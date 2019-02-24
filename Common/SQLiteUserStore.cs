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

        public event EventHandler<IUser> UserAdded;
        public void Init()
        {
            foreach (var user in LoadAllUserInfo())
            {
                _cacheDict.TryAdd(user.UserId, user);
            }
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
                _cacheDict.TryAdd(userId, userInfo);
                return userInfo;
            }

            //キャッシュにもデータベースにも無いので、新たに作成
            userInfo = new UserTest(userId);
            _cacheDict.TryAdd(userId, userInfo);
            UserAdded?.Invoke(this, userInfo);
            return userInfo;
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
                    conn.Open();
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
        private static void CreateTable(SQLiteConnection conn)
        {
            if (!TableExists(conn, tableName))
            {
                var query = $"CREATE TABLE {tableName} ({col1Name} TEXT PRIMARY KEY, {col2Name} TEXT, {col3Name} TEXT)";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
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
        public void Save()
        {
            CreateDB(_dbPath);

            //usersテーブルの前行を削除
            var deleteAllRows = $"DELETE FROM {tableName}";
            try
            {
                using (var conn = CreateConnection(_dbPath))
                using (var cmd = new SQLiteCommand(deleteAllRows, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }

            ///
            const string param1Name = "@param1";
            const string param2Name = "@param2";
            const string param3Name = "@param3";
            var query = $"INSERT INTO {tableName} ({col1Name}, {col2Name}, {col3Name}) VALUES({param1Name}, {param2Name}, {param3Name})";
            try
            {
                lock (_cacheDict)
                {
                    using (var conn = CreateConnection(_dbPath))
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        conn.Open();
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
                    conn.Open();
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
                            var nickname = reader.GetString(0);
                            var update = reader.GetDateTime(1);
                            userInfo = new UserTest(userId) { Nickname = nickname };
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
                CreateTable(conn);
            }
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
                cmd.Parameters.Add(new SQLiteParameter(param1Name, tableName));

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }
    }
    public class SQLiteUserStoreOld : IUserStore
    {
        //テーブルの項目をuser_id、json、更新日時の3つにしようか
        //名前にICONを使っている場合を考慮してIEnumerable<IMessagePart>をそのまま保存したい。
        //
        private readonly string _dbPath;
        private readonly ILogger _logger;
        private readonly Dictionary<string, IUser> _cacheDict = new Dictionary<string, IUser>();

        public event EventHandler<IUser> UserAdded;
        public void Init()
        {
            foreach (var user in LoadAllUserInfo())
            {
                _cacheDict.Add(user.UserId, user);
            }
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
                _cacheDict.Add(userId, userInfo);
                return userInfo;
            }

            //キャッシュにもデータベースにも無いので、新たに作成
            userInfo = new UserTest(userId);
            _cacheDict.Add(userId, userInfo);
            UserAdded(this, userInfo);
            return userInfo;
        }
        private List<IUser> LoadAllUserInfo()
        {
            CreateDB(_dbPath);

            var query = $"SELECT {col1Name},{col2Name},{col4Name},{col5Name},{col6Name},{col3Name} FROM {tableName}";
            var list = new List<IUser>();
            try
            {
                using (var conn = CreateConnection(_dbPath))
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var userId = reader.GetString(0);
                                string nickname;
                                if (!reader.IsDBNull(1))
                                {
                                    nickname = reader.GetString(1);
                                }
                                else
                                {
                                    nickname = null;
                                }
                                string backColorArgb;
                                if (!reader.IsDBNull(2))
                                {
                                    backColorArgb = reader.GetString(2);
                                }
                                else
                                {
                                    backColorArgb = null;
                                }
                                string foreColorArgb;
                                if (!reader.IsDBNull(3))
                                {
                                    foreColorArgb = reader.GetString(3);
                                }
                                else
                                {
                                    foreColorArgb = null;
                                }
                                var isNg = reader.GetInt32(4) == 0 ? false : true;
                                var update = reader.GetDateTime(5);
                                list.Add(new UserTest(userId) { Nickname = nickname, BackColorArgb = backColorArgb, ForeColorArgb = foreColorArgb, IsNgUser = isNg });
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
        public SQLiteUserStoreOld(string dbPath, ILogger logger)
        {
            _dbPath = dbPath;
            _logger = logger;
        }

        const string tableName = "users";
        const string col1Name = "userid";
        const string col2Name = "nickname";
        const string col4Name = "backcolor";
        const string col5Name = "forecolor";
        const string col6Name = "is_ng";
        const string col3Name = "modified";

        private static SQLiteConnection CreateConnection(string dbPath)
        {
            return new SQLiteConnection($"Data Source={dbPath}");
        }
        private static void CreateTable(SQLiteConnection conn)
        {
            if (!TableExists(conn, tableName))
            {
                var query = $"CREATE TABLE {tableName} ({col1Name} TEXT PRIMARY KEY, {col2Name} TEXT, {col4Name} TEXT, {col5Name} TEXT, {col6Name} INTEGER, {col3Name} TEXT)";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Save()
        {
            CreateDB(_dbPath);

            //usersテーブルの前行を削除
            var deleteAllRows = $"DELETE FROM {tableName}";
            try
            {
                using (var conn = CreateConnection(_dbPath))
                using (var cmd = new SQLiteCommand(deleteAllRows, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }

            ///
            const string param1Name = "@param1";
            const string param2Name = "@param2";
            const string param4Name = "@param4";
            const string param5Name = "@param5";
            const string param6Name = "@param6";
            const string param3Name = "@param3";
            var query = $"INSERT INTO {tableName} ({col1Name}, {col2Name}, {col4Name}, {col5Name}, {col6Name}, {col3Name}) VALUES({param1Name}, {param2Name}, {param4Name}, {param5Name}, {param6Name}, {param3Name})";
            try
            {
                using (var conn = CreateConnection(_dbPath))
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    conn.Open();
                    using (cmd.Transaction = conn.BeginTransaction())
                    {
                        foreach (var kv in _cacheDict)
                        {
                            var user = kv.Value;

                            //変更が無い場合は記録しない
                            if (!IsModified(user)) continue;
                            cmd.Parameters.Add(new SQLiteParameter(param1Name, user.UserId));
                            cmd.Parameters.Add(new SQLiteParameter(param2Name, user.Nickname));
                            //背景色
                            cmd.Parameters.Add(new SQLiteParameter(param4Name, user.BackColorArgb));
                            //文字色
                            cmd.Parameters.Add(new SQLiteParameter(param5Name, user.ForeColorArgb));
                            //IsNGUser
                            cmd.Parameters.Add(new SQLiteParameter(param6Name, user.IsNgUser ? 1 : 0));
                            //
                            cmd.Parameters.Add(new SQLiteParameter(param3Name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

                            cmd.ExecuteNonQuery();
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
                    conn.Open();
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
                            var nickname = reader.GetString(0);
                            var update = reader.GetDateTime(1);
                            userInfo = new UserTest(userId) { Nickname = nickname };
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
                CreateTable(conn);
            }
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
                cmd.Parameters.Add(new SQLiteParameter(param1Name, tableName));

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }
    }
}