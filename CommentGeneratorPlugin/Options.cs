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
namespace CommentViewer.Plugin
{
    [DataContract]
    public class Options
    {
        [DataMember]
        private bool _isEnabled = false;
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
        private string _hcgSettingFilePath = 
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"hcg\setting.xml");
        /// <summary>
        /// HTML5コメジェネの設定ファイルの場所
        /// </summary>
        public string HcgSettingFilePath
        {
            get { return _hcgSettingFilePath; }
            set
            {
                _hcgSettingFilePath = value;
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
            catch (Exception) { }
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
