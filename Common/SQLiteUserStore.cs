using Common;
using SitePlugin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;

namespace Common
{
    public class SQLiteUserStore : IUserStore
    {
        //テーブルの項目をuser_id、json、更新日時の3つにしようか
        //名前にICONを使っている場合を考慮してIEnumerable<IMessagePart>をそのまま保存したい。
        //
        private readonly string _dbPath;
        private readonly ILogger _logger;
        //private readonly Dictionary<string, IUser> _cacheDict = new Dictionary<string, IUser>();
        private readonly ConcurrentDictionary<string,IUser> _cacheDict = new ConcurrentDictionary<string, IUser>();
        private static readonly object _createTableLockObject = new object();
        public event EventHandler<IUser> UserAdded;
        public void Init()
        {
            //foreach (var user in LoadAllUserInfo())
            //{
            //    _cacheDict.TryAdd(user.UserId, user);
            //}
        }
        public void ClearCache()
        {
            _cacheDict.Clear();
        }
        public IEnumerable<IUser> GetAllUsers()
        {
            return _cacheDict.Values;
        }
        public IUser GetUser(string userId)
        {
            //UserInfoのインスタンスは一つのuserIdにつき一つだけ作る。
            if (_cacheDict.ContainsKey(userId))
            {
                return _cacheDict[userId];
            }

            //TODO:最初にデータベース上のユーザ情報を全てメモリに読み込むべきでは？その方が絶対に効率がいい。
            if (TryGet(userId, out IUser userInfo))
            {
                AddUser(userInfo);
                return userInfo;
            }

            //キャッシュにもデータベースにも無いので、新たに作成
            userInfo = new UserTest(userId);
            AddUser(userInfo);
            return userInfo;
        }
        private void AddUser(IUser userInfo)
        {
            if (_cacheDict.TryAdd(userInfo.UserId, userInfo))
            {
                UserAdded?.Invoke(this, userInfo);
            }
        }
        private List<IUser> LoadAllUserInfo()
        {
            CreateDB(_dbPath);

            var query = $"SELECT {col1Name},{col2Name},{col3Name} FROM {tableName}";
            var list = new List<IUser>();
            try
            {
                using (var conn = CreateConnection(_dbPath))
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    OpenConnectionSafely(conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var userId = reader.GetString(0);
                                string json = reader.GetString(1);
                                var user = FromJson(json);
                                var update = reader.GetDateTime(2);
                                list.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //ここに来るようなことは殆ど無いかと。ストレージの空き容量が無いとかそんなレベル
                //ryu_s.MyCommon.ExceptionLogger.Logging(ryu_s.MyCommon.LogLevel.error, ex);
                _logger.LogException(ex);
            }
            return list;
        }
        public SQLiteUserStore(string dbPath, ILogger logger)
        {
            _dbPath = dbPath;
            _logger = logger;
        }

        const string tableName = "users";
        const string col1Name = "userid";
        const string col2Name = "json";
        const string col3Name = "updated";

        private static SQLiteConnection CreateConnection(string dbPath)
        {
            return new SQLiteConnection($"Data Source={dbPath}");
        }
        /// <summary>
        /// usersテーブルを作成する
        /// </summary>
        /// <param name="conn"></param>
        /// <returns>テーブルを作成したか。既に存在していた場合は作成せずfalseを返す</returns>
        private static bool CreateUsersTable(SQLiteConnection conn)
        {
            lock (_createTableLockObject)
            {
                if (TableExists(conn, tableName))
                {
                    return false;
                }
                else
                {
                    var query = $"CREATE TABLE {tableName} ({col1Name} TEXT PRIMARY KEY, {col2Name} TEXT, {col3Name} TEXT)";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
        }
        static string EscapeForJson(string s)
        {
            return Newtonsoft.Json.JsonConvert.ToString(s);
        }
        private static string ToJson(IEnumerable<IMessagePart> items)
        {
            if (items == null) return "null";
            var list = new List<string>();
            foreach (var item in items)
            {
                if (item is IMessageText text)
                {
                    var s = $"{{\"type\":\"text\",\"value\":{EscapeForJson(text.Text)}}}";
                    list.Add(s);
                }
            }
            return "[" + string.Join(",", list) + "]";
        }
        public static string ToJson(string str)
        {
            return str == null ? "null" : $"\"{str}\"";
        }
        public static string ToJson(IUser user)
        {
            var json = "{"
                + $"\"userid\":\"{user.UserId}\","
                + $"\"name\":{ToJson(user.Name)},"
                + $"\"nickname\":{ToJson(user.Nickname)},"
                + $"\"backcolor\":{ToJson(user.BackColorArgb)},"
                + $"\"forecolor\":{ToJson(user.ForeColorArgb)},"
                + $"\"is_ng\":\"{user.IsNgUser}\""
                + "}";
            return json;
        }
        public static IUser FromJson(string json)
        {
            dynamic d = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var userId = d.userid.Value;
            var nick = d.nickname.Value;
            var backColor = d.backcolor.Value;
            var foreColor = d.forecolor.Value;
            var isNg = d.is_ng.Value.ToLower() == "true";
            var name = d.name;
            var nameItems = new List<IMessagePart>();
            if (name != null)
            {
                foreach (var nameItem in name)
                {
                    if (nameItem.type == "text")
                    {
                        var val = nameItem.value.Value;
                        nameItems.Add(MessagePartFactory.CreateMessageText(val));
                    }
                }
            }
            var user = new UserTest(userId) { Name = nameItems, Nickname = nick, BackColorArgb = backColor, ForeColorArgb = foreColor, IsNgUser = isNg };
            return user;
        }
        /// <summary>
        /// 空のusersテーブルに_cacheDictに登録されたユーザ情報全てを挿入する
        /// </summary>
        /// <param name="conn"></param>
        private void InsertAllUsersToUsersTable(SQLiteConnection conn)
        {
            const string param1Name = "@param1";
            const string param2Name = "@param2";
            const string param3Name = "@param3";
            var query = $"INSERT INTO {tableName} ({col1Name}, {col2Name}, {col3Name}) VALUES({param1Name}, {param2Name}, {param3Name})";
            try
            {
                lock (_cacheDict)
                {
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        OpenConnectionSafely(conn);
                        using (cmd.Transaction = conn.BeginTransaction())
                        {
                            foreach (var kv in _cacheDict)
                            {
                                var user = kv.Value;
                                //変更が無い場合は記録しない
                                if (!IsModified(user)) continue;

                                var json = ToJson(user);

                                /*
                                 * {
                                 *      "Name": [
                                 *          {
                                 *              "type":"Text",
                                 *              "Value":"Ryu",
                                 *          },
                                 *          {
                                 *              "type":"RemoteIcon",
                                 *              "url":"",
                                 *          }
                                 *      ]
                                 * }
                                 * 
                                 */

                                cmd.Parameters.Add(new SQLiteParameter(param1Name, user.UserId));
                                cmd.Parameters.Add(new SQLiteParameter(param2Name, json));
                                //
                                cmd.Parameters.Add(new SQLiteParameter(param3Name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

                                cmd.ExecuteNonQuery();
                            }
                            cmd.Transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        public void Save()
        {
            using (var conn = CreateConnection(_dbPath))
            {
                //テーブルが既に存在したらユーザID毎にレコードの存在をチェックしながらupdateかinsertをする。
                //テーブルが無ければ全部insert
                if (CreateUsersTable(conn))
                {
                    InsertAllUsersToUsersTable(conn);
                }
                else
                {
                    UpdateUsers(conn);
                }
            }
        }
        const string Param1Name = "@param1";
        const string Param2Name = "@param2";
        const string Param3Name = "@param3";
        const string UpdateQuery = "UPDATE "+tableName+" SET "+col2Name+"="+Param2Name+","+col3Name+"="+Param3Name+
                " WHERE "+col1Name+"="+Param1Name;
        const string InsertQuery = "INSERT INTO " + tableName + " (" + col1Name + ", " + col2Name + ", " + col3Name + ") VALUES(" + Param1Name + ", " + Param2Name + ", " + Param3Name + ")";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="user"></param>
        /// <returns>影響があった行数</returns>
        private int UpdateUser(SQLiteCommand cmd, IUser user)
        {
            var json = ToJson(user);

            /*
             * {
             *      "Name": [
             *          {
             *              "type":"Text",
             *              "Value":"Ryu",
             *          },
             *          {
             *              "type":"RemoteIcon",
             *              "url":"",
             *          }
             *      ]
             * }
             * 
             */
            cmd.CommandText = UpdateQuery;
            cmd.Parameters.Add(new SQLiteParameter(Param1Name, user.UserId));
            cmd.Parameters.Add(new SQLiteParameter(Param2Name, json));
            //
            cmd.Parameters.Add(new SQLiteParameter(Param3Name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            return cmd.ExecuteNonQuery();
        }
        private void InsertUser(SQLiteCommand cmd, IUser user)
        {
            var json = ToJson(user);

            /*
             * {
             *      "Name": [
             *          {
             *              "type":"Text",
             *              "Value":"Ryu",
             *          },
             *          {
             *              "type":"RemoteIcon",
             *              "url":"",
             *          }
             *      ]
             * }
             * 
             */
            cmd.CommandText = InsertQuery;
            cmd.Parameters.Add(new SQLiteParameter(Param1Name, user.UserId));
            cmd.Parameters.Add(new SQLiteParameter(Param2Name, json));
            //
            cmd.Parameters.Add(new SQLiteParameter(Param3Name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// usersテーブルのユーザ情報を_cacheDictのものに書き換える。
        /// ただし、_cacheDictにあってusersには無いユーザはINSERTする。
        /// </summary>
        /// <param name="conn"></param>
        private void UpdateUsers(SQLiteConnection conn)
        {
            lock (_cacheDict)
            {
                using (var cmd = new SQLiteCommand(conn))
                {
                    OpenConnectionSafely(conn);
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (var kv in _cacheDict)
                        {
                            var user = kv.Value;

                            var lineCount = UpdateUser(cmd, user);
                            if (lineCount == 0)
                            {
                                //このユーザのデータはテーブル上には無いからInsertする
                                InsertUser(cmd, user);
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
        }
        /// <summary>
        /// userに変更が加えられているか
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool IsModified(IUser user)
        {
            return !string.IsNullOrEmpty(user.Nickname) || !string.IsNullOrEmpty(user.ForeColorArgb) || !string.IsNullOrEmpty(user.BackColorArgb) || user.IsNgUser;
        }

        private bool Exists(string userId)
        {
            return TryGet(userId, out var userInfo);
        }
        private bool TryGet(string userId, out IUser userInfo)
        {
            CreateDB(_dbPath);

            const string param1Name = "@param1";
            var query = $"SELECT {col2Name},{col3Name} FROM {tableName} WHERE {col1Name}={param1Name}";

            try
            {
                using (var conn = CreateConnection(_dbPath))
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    OpenConnectionSafely(conn);
                    cmd.Parameters.Add(new SQLiteParameter(param1Name, userId));
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            userInfo = null;
                            return false;
                        }
                        else
                        {
                            reader.Read();
                            var json = reader.GetString(0);
                            //2021/11/30
                            //"文字列は有効な DateTime ではありませんでした"という例外が投げられた。
                            //どうせ使っていないから削除したいけど、原因不明だからとりあえずコメントアウト
                            //var update = reader.GetDateTime(1);
                            userInfo = FromJson(json);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //ここに来るようなことは殆ど無いかと。ストレージの空き容量が無いとかそんなレベル
                _logger.LogException(ex);
                userInfo = null;
                return false;
            }
        }
        private static bool DBExists(string dbPath)
        {
            return System.IO.File.Exists(dbPath);
        }
        /// <summary>
        /// DBファイルが無ければ作成する。
        /// </summary>
        /// <param name="dbPath"></param>
        private static void CreateDB(string dbPath)
        {
            using (var conn = CreateConnection(dbPath))
            {
                conn.Open();
                CreateUsersTable(conn);
            }
        }
        /// <summary>
        /// ConnectionをOpen()する
        /// 既にOpenの場合は何もしない
        /// </summary>
        /// <param name="conn"></param>
        private static void OpenConnectionSafely(SQLiteConnection conn)
        {
            if (!IsOpen(conn))
            {
                conn.Open();
            }
        }
        private static bool IsOpen(SQLiteConnection conn)
        {
            return conn != null && conn.State.HasFlag(System.Data.ConnectionState.Open);
        }
        private static bool TableExists(SQLiteConnection conn, string tableName)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            const string param1Name = "@param1";
            string query = $"SELECT name FROM sqlite_master WHERE type='table' AND name= {param1Name}";

            using (var cmd = new SQLiteCommand(query, conn))
            {
                OpenConnectionSafely(conn);
                cmd.Parameters.Add(new SQLiteParameter(param1Name, tableName));

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }
    }
}