using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using System.Runtime.Serialization;
namespace BeamCommentViewer.Plugin
{
    [DataContract]
    public class Options
    {
        [DataMember]
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged();
            }
        }
        [DataMember]
        private string _bouyomiChanPath;
        public string BouyomiChanPath
        {
            get { return _bouyomiChanPath; }
            set
            {
                _bouyomiChanPath = value;
                RaisePropertyChanged();
            }
        }
        [DataMember]
        private bool _isReadHandleName = true;
        public bool IsReadHandleName
        {
            get { return _isReadHandleName; }
            set
            {
                _isReadHandleName = value;
                RaisePropertyChanged();
            }
        }
        [DataMember]
        private bool _isReadComment = true;
        public bool IsReadComment
        {
            get { return _isReadComment; }
            set
            {
                _isReadComment = value;
                RaisePropertyChanged();
            }
        }
        [DataMember]
        private bool _isAppendNickTitle = true;
        /// <summary>
        /// コテハンに敬称を付加して読み上げるか
        /// </summary>
        public bool IsAppendNickTitle
        {
            get { return _isAppendNickTitle; }
            set
            {
                _isAppendNickTitle = value;
                RaisePropertyChanged();
            }
        }
        [DataMember]
        private string _nickTitle = "さん";
        /// <summary>
        /// コテハンに付加する敬称
        /// </summary>
        public string NickTitle
        {
            get { return _nickTitle; }
            set
            {
                _nickTitle = value;
                RaisePropertyChanged();
            }
        }

        public static void Save(Options options, string filePath)
        {
            var serializer = new DataContractSerializer(typeof(Options));
            using (var sw = System.Xml.XmlWriter.Create(filePath, new System.Xml.XmlWriterSettings { Indent = true }))
            {
                serializer.WriteObject(sw, options);
            }
        }
        public static Options Load(string filePath)
        {
            Options options = null;
            try
            {
                var serializer = new DataContractSerializer(typeof(Options));
                using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                {
                    options = serializer.ReadObject(fs) as Options;
                }
            }
            catch (System.IO.FileNotFoundException) { }
            catch (Exception)            {            }
            finally
            {
                if (options == null)
                {
                    options = new Options();
                }
            }
            return options;
        }

        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
