namespace Winner.Storage.Route
{
    public interface IFileRoute
    {

        /// <summary>
        /// 生成唯一文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string CreateFileName(string fileName);
   
    }
}
