using System;
using System.Collections.Generic;
using System.Linq;
using Winner.Persistence.Relation;


namespace Winner.Persistence.Compiler.Analysis
{
    public class StandardAnalyzer :  IAnalyzer
    {
    
        #region 属性
        /// <summary>
        /// 
        /// </summary>
        public virtual int MinLength { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxLength { get; set; }
        /// <summary>
        /// 分组主词库
        /// </summary>
        protected IDictionary<int, WordInfo[]> MainGroupDictionaries { get; set; }
        private WordInfo[] _mainDictionaries=new WordInfo[0];
        /// <summary>
        /// 主词库
        /// </summary>
        public WordInfo[] MainDictionaries
        {
            get { return _mainDictionaries; }
            set {
                _mainDictionaries = value?.OrderBy(it=>it.Name).ToArray();
                MainGroupDictionaries = GetGroupDictionaries(_mainDictionaries);
                MaxLength = MainGroupDictionaries.Keys.Max(it => it);
                MinLength = MainGroupDictionaries.Keys.Min(it => it);
            }
        }
        /// <summary>
        /// 分组主词库
        /// </summary>
        protected IDictionary<int, WordInfo[]> StopGroupDictionaries { get; set; }
        private WordInfo[] _stopDictionaries=new WordInfo[0];

        /// <summary>
        /// 禁用词库
        /// </summary>
        public WordInfo[] StopDictionaries
        {
            get { return _stopDictionaries; }
            set
            {
                _stopDictionaries  = value?.OrderBy(it=>it.Name).ToArray();
                StopGroupDictionaries = GetGroupDictionaries(_stopDictionaries);
            }
        }
        /// <summary>
        /// 分组主词库
        /// </summary>
        protected IDictionary<int, KeyValuePair<string, string>[]> TransformGroupDictionaries { get; set; }
        private KeyValuePair<string, string>[] _transformDictionaries = new KeyValuePair<string, string>[0];
        /// <summary>
        /// 转换词库
        /// </summary>
        public KeyValuePair<string, string>[] TransformDictionaries
        {
            get { return _transformDictionaries; }
            set
            {
                _transformDictionaries = value?.OrderBy(it=>it.Key).ToArray();
                TransformGroupDictionaries = GetGroupDictionaries(_transformDictionaries);
            }
        }

        private string[] _splitDictionaries = new[]
            {
               "\r\n", "\r", "\n", "！", "@", "#", "￥", "%", "……", "&", "*", "（", "）", "——", "【",
                 "】",  "；", "“", "‘", "《", "，", "》", "。", "？"
                 ,"~","`","!","#","$","%","^","&","*","(",")",":",";","'","\"",",","<",">",".","?","/","|","\\"
            };
        /// <summary>
        /// 禁用词库
        /// </summary>
        public string[] SplitDictionaries
        {
            get { return _splitDictionaries; }
            set
            {
                _splitDictionaries = value;
            }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public StandardAnalyzer()
        {
          
        }

        /// <summary>
        /// 拆分字符串,词库实例,拼音实例
        /// </summary>
        /// <param name="mainDictionaries"></param>
        /// <param name="stopDictionaries"></param>
        /// <param name="rootDictionaries"></param>
        public StandardAnalyzer(WordInfo[] mainDictionaries, WordInfo[] stopDictionaries, KeyValuePair<string, string>[] rootDictionaries)
        {
            MainDictionaries = mainDictionaries;
            StopDictionaries = stopDictionaries;
            TransformDictionaries = rootDictionaries;
        }
        #endregion

        #region 接口的实现

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="property"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual IList<WordInfo> Resolve(OrmPropertyInfo property, string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            input = input.ToLower();
            var words = Resolve(input);
            return words;
        }
        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual IList<WordInfo> Resolve(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            var keys = SplitEnglishOrNumber(input);
            var words = new List<WordInfo>();
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    words.AddRange(GetWords(key));
                }
            }
            return words;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual IList<WordInfo> GetWords(string key)
        {
            var words = new List<WordInfo>();
            words.AddRange(Tokenize(key));
            foreach (var word in words)
            {
                Lemmatize(word);
            }
            return FilterStop(words);
        }
        #endregion

        #region 过滤禁用词
        /// <summary>
        /// 过滤禁用词
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        protected virtual IList<WordInfo> FilterStop(IList<WordInfo> terms)
        {
            return terms.Where(word => !IsStopWord(word.Name)).ToList();
        }

        /// <summary>
        /// 是否为禁用词
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual bool IsStopWord(string name)
        {
            if (StopGroupDictionaries == null)
                return false;
            if (!StopGroupDictionaries.ContainsKey(name.Length))
                return false;
            var dictionaries = StopGroupDictionaries[name.Length];
            int low = 0, high = dictionaries.Length - 1;
            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                if (dictionaries[mid].Name.Equals(name))//找到该词
                    return true;
                if (dictionaries[mid].Name.CompareTo(name) > 0)
                    high = mid - 1;
                else
                    low = mid + 1;
            }
            return false;
        }
        #endregion

        #region 转换元词
      
        /// <summary>
        /// 转换词根
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        protected virtual bool Lemmatize(WordInfo word)
        {
            if (TransformGroupDictionaries == null)
                return false;
            if (!TransformGroupDictionaries.ContainsKey(word.Name.Length))
                return false;
            var dictionaries = TransformGroupDictionaries[word.Name.Length];
            int low = 0, high = dictionaries.Length - 1;
            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                if (dictionaries[mid].Key.Equals(word.Name)) //找到该词
                {
                    word.Name = dictionaries[mid].Value;
                    return true;  
                }

                if (dictionaries[mid].Key.CompareTo(word.Name) > 0)
                    high = mid - 1;
                else
                    low = mid + 1;
            }
            return false;
        }

        #endregion

        #region 得到Token
     
        /// <summary>
        /// 得到词源
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected virtual IList<WordInfo> Tokenize(string key)
        {
            if(MainGroupDictionaries==null)
                return new List<WordInfo>{new WordInfo{Name= key } };
            var terms = new List<WordInfo>();
            if (MainGroupDictionaries.Count > 0)
            {
                AddTermsByForward(terms, key, MaxLength, MinLength);
            }
            return terms;
        }
        /// <summary>
        /// 正向最大匹配,返回切片
        /// </summary>
        /// <param name="terms"></param>
        /// <param name="key"></param>
        /// <param name="maxLength"></param>
        /// <param name="minLength"></param>
        /// <returns></returns>
        protected virtual void AddTermsByForward(IList<WordInfo> terms, string key, int maxLength, int minLength)
        {
            int index = 0;
            while (index < key.Length)
            {
                var length = index + maxLength > key.Length ? key.Length - index : maxLength;
                if (length < minLength)
                    break;
                var name = key.Substring(index, length);
                int i = name.Length;
                for (; i > 0; i--)
                {
                    if (i < minLength)
                    {
                        i = 0;
                        break;
                    }
                    string word = name.Substring(0, i);
                    var wd = CheckMainWord(word);
                    if (wd != null)
                    {
                        terms.Add(new WordInfo {Name = word,Index=index,Tag=wd.Tag});
                        break;
                    }
                }
                index += (i == 0 ? 1 : i);
            }
        }

        /// <summary>
        /// 反向最大匹配,返回切片
        /// </summary>
        /// <param name="terms"></param>
        /// <param name="key"></param>
        /// <param name="maxLength"></param>
        /// <param name="minLength"></param>
        /// <returns></returns>
        protected virtual void AddTokensByOpposite(IList<WordInfo> terms, string key, int maxLength, int minLength)
        {
            int index = key.Length;
            while (index >= 0)
            {
                var length = index - maxLength < 0 ? index : maxLength;
                if (length < minLength)
                    break;
                var name = key.Substring(0, length);
                int i = 0;
                for (; i < name.Length; i++)
                {
                    if (name.Length - i < minLength)
                    {
                        i = index;
                        break;
                    }
                    string word = name.Substring(i, name.Length - i);
                    var wd = CheckMainWord(word);
                    if (wd!=null)
                    {
                        terms.Add(new WordInfo {Name = word,Index = index,Tag= wd.Tag});
                        break;
                    }
                }
                index = (i == index ? index-1 : i);
            }
        }

     

        /// <summary>
        /// 是否为词
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual WordInfo CheckMainWord(string name)
        {
            if (MainGroupDictionaries == null)
                return null;
            if (!MainGroupDictionaries.ContainsKey(name.Length))
                return null;
            var dictionaries = MainGroupDictionaries[name.Length];
            int low = 0, high = dictionaries.Length - 1;
            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                if (dictionaries[mid].Name.Equals(name))//找到该词
                    return dictionaries[mid];
                if (dictionaries[mid].Name.CompareTo(name) > 0)
                    high = mid - 1;
                else
                    low = mid + 1;
            }
            return null;
        }
        #endregion

        #region 拆分

        /// <summary>
        /// 把英文或者数字当作一个整体保留并把前后的中文切开
        /// </summary>
        /// <param name="input"></param>
        protected virtual IList<string> SplitEnglishOrNumber(string input)
        {
            //IList<string> keys = new List<string>();
            var array = input.Split(SplitDictionaries, StringSplitOptions.None);
            //foreach (var arr in array)
            //{
            //    if (string.IsNullOrEmpty(arr)) continue;
            //    for (int i = 0; i < arr.Length; i++)
            //    {
            //        if (arr[i] <= 127 && !IsEnglishOrNumber(arr[i])) continue;
            //        var builder = new StringBuilder();
            //        while (i < arr.Length && IsEnglishOrNumber(arr[i]))
            //        {
            //            builder.Append(arr[i]);
            //            i++;
            //        }
            //        if (builder.Length > 0) keys.Add(builder.ToString());
            //        if (i >= arr.Length || arr[i] <= 127 && !IsEnglishOrNumber(arr[i])) continue;
            //        builder = new StringBuilder();
            //        while (i < arr.Length && arr[i] > 127)
            //        {
            //            builder.Append(arr[i]);
            //            i++;
            //        }
            //        if (builder.Length > 0)
            //        {
            //            keys.Add(builder.ToString());
            //            i--;
            //        }
            //    }
            //}
            return array;
        }

        /// <summary>
        /// 是否是英文或者数字
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected virtual bool IsEnglishOrNumber(char c)
        {
            if (c >= 48 && c <= 57 || c >= 65 && c <= 90 || c == 95 || c >= 97 && c <= 122)
                return true;
            return false;
        }
        #endregion

        #region 根据词长度分组
        /// <summary>
        /// 根据词分组
        /// </summary>
        /// <param name="dictionaries"></param>
        /// <returns></returns>
        protected virtual IDictionary<int, WordInfo[]> GetGroupDictionaries(WordInfo[] dictionaries)
        {
            var tempGroupDictionary = new Dictionary<int, IList<WordInfo>>();
            foreach (var key in dictionaries)
            {
                if (!tempGroupDictionary.ContainsKey(key.Name.Length))
                {
                    tempGroupDictionary.Add(key.Name.Length, new List<WordInfo>());
                }
                tempGroupDictionary[key.Name.Length].Add(key);
            }
            var groupDictionary = new Dictionary<int, WordInfo[]>();
            foreach (var temp in tempGroupDictionary)
            {
                groupDictionary.Add(temp.Key,temp.Value.ToArray());
            }
            return groupDictionary;
        }
        /// <summary>
        /// 根据词分组
        /// </summary>
        /// <param name="dictionaries"></param>
        /// <returns></returns>
        protected virtual IDictionary<int, KeyValuePair<string, string>[]> GetGroupDictionaries(KeyValuePair<string, string>[] dictionaries)
        {
            var tempGroupDictionary = new Dictionary<int, IList<KeyValuePair<string, string>>>();
            foreach (var dic in dictionaries)
            {
                if (!tempGroupDictionary.ContainsKey(dic.Key.Length))
                {
                    tempGroupDictionary.Add(dic.Key.Length, new List<KeyValuePair<string, string>>());
                }
                tempGroupDictionary[dic.Key.Length].Add(new KeyValuePair<string, string>(dic.Key,dic.Value));
            }
            var groupDictionary = new Dictionary<int, KeyValuePair<string, string>[]>();
            foreach (var temp in tempGroupDictionary)
            {
                groupDictionary.Add(temp.Key, temp.Value.ToArray());
            }
            return groupDictionary;
        }
        #endregion
    }
}
