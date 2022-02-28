namespace Winner.Creation
{


    public interface IFactory
    {
        /// <summary>
        /// 创建对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>(string name=null);
        /// <summary>
        /// 设置对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <param name="isSingle"></param>
        bool Set(string name, object obj, bool isSingle);

        /// <summary>
        /// 设置对象
        /// </summary>
        /// <param name="info"></param>
        bool Set(FactoryInfo info);

    }
}
