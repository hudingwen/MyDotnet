using AppStoreConnect.Model;
using MyDotnet.Domain.Dto.Com;
using MyDotnet.Domain.Dto.ExceptionDomain;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services.System;
using System.IO;
using System.Linq;
using System.Runtime;

namespace MyDotnet.Services.Com
{
    public class FileManagerService : BaseServices<BaseEntity>
    {
        BaseServices<Department> _departmentServices;
        public DicService _dicService; 
        public AspNetUser _user;
        public FileManagerService(BaseRepository<BaseEntity> baseRepository
            ,DicService dicService
            ,AspNetUser user
            ,BaseServices<Department> departmentServices) 
            : base(baseRepository)
        {
            _dicService = dicService;
            _user = user;
            _departmentServices = departmentServices;
        }

        public async Task<FileManagerInfo> GetList(SearchFileDto searchFile)
        {
            FileManagerInfo fileManagerInfo = new FileManagerInfo();

            List<FileManagerInfo> fileList = fileManagerInfo.fileList;

            var rootDic = await _dicService.GetDicDataOne(FileManagerConfig.KEY, FileManagerConfig.rootDic);
            var rootpath  = Path.Combine(rootDic.content, _user.DeptId.ObjToString());

            if (!Directory.Exists(rootpath))
            {
                Directory.CreateDirectory(rootpath); 
            }
            //自己
            DirectoryInfo directoryInfo = new DirectoryInfo(rootpath);

            //所有部门
            DirectoryInfo directoryInfoRoot = new DirectoryInfo(rootDic.content);
            var pathRoots = directoryInfoRoot.FullName.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            var pathRootsTemp = directoryInfoRoot.FullName.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();

            
            if (searchFile != null && searchFile.parent != null && !string.IsNullOrEmpty(searchFile.parent.filePath))
            { 
                await CheckUserDic(searchFile.parent.filePath);
                //筛选
                directoryInfo = new DirectoryInfo(searchFile.parent.filePath);
            }
            var pathParts = directoryInfo.FullName.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            var pathPrefix = pathParts.Skip(pathRoots.Count).Take(pathParts.Count - pathRoots.Count).ToArray();


            var dicRoot = new DirectoryInfo(string.Join("/", pathRoots));
            var dicInfoRoot = new FileManagerInfo();
            HandleDicInfo(dicInfoRoot, dicRoot);


            fileManagerInfo.barList.Add(new BarDicInfo { name = "根目录",dicInfo = dicInfoRoot });


             
            for (int i = 0; i < pathPrefix.Length; i++)
            {
                pathRootsTemp.Add(pathPrefix[i]);

                var dic = new DirectoryInfo(string.Join("/", pathRootsTemp));
                var dicInfo = new FileManagerInfo();
                HandleDicInfo(dicInfo, dic);
                var barInfo = new BarDicInfo { name = pathPrefix[i], dicInfo = dicInfo };
                if (i == 0)
                {
                    //各部门
                    var depInfo = await _departmentServices.Dal.QueryById(barInfo.dicInfo.fileName);
                    if(depInfo != null)
                    {
                        barInfo.name = depInfo.Name;
                    } 
                }
                fileManagerInfo.barList.Add(barInfo);
            }

            FileInfo[] files = directoryInfo.GetFiles((searchFile == null || string.IsNullOrEmpty(searchFile.key) ? "" : $"*{searchFile.key}*"));
            DirectoryInfo[] dics = directoryInfo.GetDirectories((searchFile == null || string.IsNullOrEmpty(searchFile.key) ? "" : $"*{searchFile.key}*"));

            var isRoot = await CheckRootDic(directoryInfo.FullName);
            var isCurrentRoot = await CheckRootDicIsMyDep(directoryInfo.FullName, _user.DeptId.ObjToString());

            //根目录 
            HandleDicInfo(fileManagerInfo, directoryInfo);
            if(isRoot)  fileManagerInfo.fileDisplay = "根目录";
            if (isCurrentRoot)
            {
                //各部门
                var depInfo = await _departmentServices.Dal.QueryById(fileManagerInfo.fileName);
                if (depInfo != null)
                {
                    fileManagerInfo.fileDisplay = depInfo.Name;
                }
            }
            //文件夹
            foreach (var item in dics)
            {
                var dicInfo = new FileManagerInfo();
                HandleDicInfo(dicInfo, item);
                if (isRoot)
                {

                    var depInfo = await _departmentServices.Dal.QueryById(dicInfo.fileName);
                    if (depInfo != null)
                    {
                        dicInfo.fileDisplay = depInfo.Name;
                    }
                }
                fileList.Add(dicInfo);
            }
            //文件
            foreach (var item in files)
            {
                var fileInfo = new FileManagerInfo();
                HandleFileInfo(item, fileInfo);
                fileInfo.isLeaf = true;
                fileInfo.disabled = true;
                fileList.Add(fileInfo);
                
            }
            await Task.CompletedTask;
            return fileManagerInfo;
        }

        public async Task CheckUserDic(string filePath,string deptId = "")
        {

            var rootDic = await _dicService.GetDicDataOne(FileManagerConfig.KEY, FileManagerConfig.rootDic);
            var rootpath = "";
            if (string.IsNullOrEmpty(deptId))
            {
                //验证根目录
                rootpath = Path.Combine(rootDic.content);
            }
            else
            {
                //验证自己
                rootpath = Path.Combine(rootDic.content, deptId);
            }

            var child = Path.GetFullPath(filePath);
            var parent = Path.GetFullPath(rootpath);
            if (!child.StartsWith(parent))
            {
                throw new ServiceException("无权访问");
            }
        }
        public async Task<bool> CheckRootDic(string filePath)
        {

            var rootDic = await _dicService.GetDicDataOne(FileManagerConfig.KEY, FileManagerConfig.rootDic);
            var rootpath = rootDic.content;

            var child = Path.GetFullPath(filePath);
            var parent = Path.GetFullPath(rootpath);
            if (child.Equals(parent))
            {
                return true;
            }
            return false;
        }
        public async Task<bool> CheckRootDicIsMyDep(string filePath, string deptId)
        {

            var rootDic = await _dicService.GetDicDataOne(FileManagerConfig.KEY, FileManagerConfig.rootDic);
            var rootpath = Path.Combine(rootDic.content, deptId);

            var child = Path.GetFullPath(filePath);
            var parent = Path.GetFullPath(rootpath);
            if (child.Equals(parent))
            {
                return true;
            }
            return false;
        }

        private static void HandleFileInfo(FileInfo item, FileManagerInfo fileInfo)
        {
            fileInfo.fileType = "file";
            fileInfo.fileName = item.Name;

            fileInfo.fileDisplay = item.Name;
            fileInfo.fileExt = item.Extension;

            fileInfo.fileParentDic = item.Directory.FullName;
            fileInfo.filePath = item.FullName;
            fileInfo.fileSize = item.Length;
            fileInfo.fileCreateTime = item.CreationTime;
            fileInfo.fileEditTime = item.LastWriteTime;
        }

        private static void HandleDicInfo(FileManagerInfo fileManagerInfo, DirectoryInfo directoryInfo)
        {
            fileManagerInfo.fileType = "folder";
            fileManagerInfo.fileName = directoryInfo.Name;
            fileManagerInfo.fileDisplay = directoryInfo.Name;
            fileManagerInfo.fileParentDic = directoryInfo.Parent.FullName;
            fileManagerInfo.filePath = directoryInfo.FullName;
            fileManagerInfo.fileCreateTime = directoryInfo.CreationTime;
            fileManagerInfo.fileEditTime = directoryInfo.LastWriteTime;
        }

        internal async Task DeleteFile(string filePath)
        {
            await CheckUserDic(filePath, _user.DeptId.ObjToString());
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath);
            }
            await Task.CompletedTask;
        }

        public async Task CreateDic(NewDicDto newDic)
        {

            await CheckUserDic(newDic.parent.filePath,_user.DeptId.ObjToString());

            var dic = Path.Combine(newDic.parent.filePath, newDic.fileName);
            if (Directory.Exists(dic))
            {
                throw new ServiceException("文件夹已存在,无需重复创建");
            }
            Directory.CreateDirectory(dic); 
        }
    }
}
