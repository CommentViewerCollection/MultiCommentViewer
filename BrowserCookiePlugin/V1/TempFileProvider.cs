using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ryu_s.BrowserCookie
{
    /// <summary>
    /// 一時ファイルを扱いやすく。
    /// インスタンスを作成すると、ファイルも作成され、インスタンスを破棄すると、ファイルも削除される。
    /// </summary>
    public class TempFileProvider : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public TempFileProvider()
        {
            Create();
        }
        /// <summary>
        /// パス
        /// </summary>
        public string Path
        {
            get;
            private set;
        }
        /// <summary>
        /// 作成する。
        /// </summary>
        /// <returns></returns>
        private bool Create()
        {
            Path = System.IO.Path.GetTempFileName();
            System.Threading.Thread.Sleep(5);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool Delete()
        {
            try
            {
                System.IO.File.Delete(Path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return true;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                Delete();
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~TempFileProvider()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // : uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
