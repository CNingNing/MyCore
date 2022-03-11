namespace Winner.Reverse
{


    public interface IMapper 
    {
        
        /// <summary>
        /// map����
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        TDestination Map<TDestination>(object source);

        /// <summary>
        /// map����
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    }
  
}
