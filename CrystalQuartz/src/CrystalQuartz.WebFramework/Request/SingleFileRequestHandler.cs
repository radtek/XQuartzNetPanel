namespace CrystalQuartz.WebFramework.Request
{
    using System.Reflection;
    using CrystalQuartz.WebFramework.HttpAbstractions;

    public class SingleFileRequestHandler : AbstractFileRequestHandler
    {
        private readonly string _path;

        /// <summary>
        /// 单个文件请求处理程序
        /// </summary>
        /// <param name="resourcesAssembly"></param>
        /// <param name="resourcePrefix"></param>
        /// <param name="path"></param>
        public SingleFileRequestHandler(Assembly resourcesAssembly, string resourcePrefix, string path) : base(resourcesAssembly, resourcePrefix)
        {
            _path = path;
        }

        protected override string GetPath(IRequest request)
        {
            return _path;
        }
    }
}