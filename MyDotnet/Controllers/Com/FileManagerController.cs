using AppStoreConnect.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using MyDotnet.Common.Cache;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.Com;
using MyDotnet.Domain.Dto.ExceptionDomain;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Helper;
using MyDotnet.Services.Com;
using MyDotnet.Services.System;
using NetTaste;
using SharpCompress.Common; 
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
namespace MyDotnet.Controllers.Com
{

    /// <summary>
    /// 文件管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class FileManagerController : BaseApiController
    {
        private FileManagerService _fileManagerService; 
        private ICaching _caching; 
        public AspNetUser _user;
        public DicService _dicService;
        public FileManagerController(FileManagerService fileManagerService
            , ICaching caching
            , AspNetUser user
            , DicService dicService)
        {
            _fileManagerService = fileManagerService;
            _caching = caching;
            _user = user;
            _dicService = dicService;
        }

        [HttpPost]
        public async Task<MessageModel<FileManagerInfo>> GetList(SearchFileDto searchFile)
        {
            FileManagerInfo data = await _fileManagerService.GetList(searchFile);
            return Success(data);
        }


        [HttpPost]
        public async Task<MessageModel> CreateDic(NewDicDto newDic)
        {
            await _fileManagerService.CreateDic(newDic); 
            return MessageModel.Success("创建成功");
        }
        [HttpPost]
        public async Task<MessageModel> DeleteFile([FromBody] FileManagerInfo fileManagerInfo)
        {
            await _fileManagerService.DeleteFile(fileManagerInfo.filePath);
            return MessageModel.Success("删除成功");
        }
        [HttpPost]
        public async Task<MessageModel> UploadFile()
        {
            var boundary = HttpContext.Request.ContentType.Split("boundary=")[1];

            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            var filePath = "";
            while (section != null)
            {

                if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                {
                    if (string.IsNullOrEmpty(contentDisposition.FileName))
                    {
                        using (var streamReader = new StreamReader(section.Body, Encoding.UTF8))
                        {
                            filePath = await streamReader.ReadToEndAsync();
                             await _fileManagerService.CheckUserDic(filePath, _user.DeptId.ObjToString());
                            //var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                            //formData[key] = value;
                        }
                    }
                    else if (contentDisposition.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(contentDisposition.FileName))
                    {
                        if (string.IsNullOrEmpty(filePath)) return MessageModel.Fail("上传文件夹不能为空");
                        var fileName = contentDisposition.FileName.Replace("\"", "");
                        var saveToPath = Path.Combine(filePath, fileName);

                        using (var targetStream = FileHelper.Create(saveToPath))
                        {
                            try
                            {
                                await section.Body.CopyToAsync(targetStream);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.logApp.Error("文件上传过程中异常", ex);
                                FileHelper.FileDel(saveToPath);
                            }
                        }
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }


            //if (file == null) return MessageModel.Fail("请选择要上传的文件");
            ////file.FileName
            //var fullPath = Path.Combine(filePath, file.FileName);
            //if (FileHelper.FileExists(fullPath))
            //{
            //    return MessageModel.Fail("文件已存在");
            //}
            //// 使用文件流保存文件到本地，防止内存溢出
            //using (var stream = new FileStream(fullPath, FileMode.Create))
            //{
            //    await file.CopyToAsync(stream);
            //}
            await Task.CompletedTask;
            return MessageModel.Success("上传成功");
        }


        [HttpPost]
        public async Task<MessageModel<string>> CreateDownloadFileInfo([FromBody] DownloadFileDto downloadFile)
        {
            await _fileManagerService.CheckUserDic(downloadFile.filePath);
            FileTokenInfo fileTokenInfo = new FileTokenInfo();
            fileTokenInfo.uid = _user.ID;
            fileTokenInfo.deptId = _user.DeptId;
            fileTokenInfo.filePath = downloadFile.filePath;

            var rootDic = await _dicService.GetDicDataOne(FileManagerConfig.KEY, FileManagerConfig.fileExpire);
            var token = StringHelper.GetGUID();
            _caching.Set<FileTokenInfo>(token, fileTokenInfo, TimeSpan.FromHours(rootDic.content.ObjToMoney()));
            return MessageModel<string>.Success("创建成功", token);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(string token)
        {
            var fileInfo = _caching.Get<FileTokenInfo>(token);
            if (fileInfo == null) throw new ServiceException("文件失效");
            string filePath = fileInfo.filePath;


            await _fileManagerService.CheckUserDic(filePath); 

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var fileName = Path.GetFileName(filePath);
            // 创建内容类型提供者
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            // 尝试获取文件的内容类型，如果获取失败则使用默认类型
            if (!provider.TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream";
            }

            await Task.CompletedTask;
            return File(fileStream, contentType, fileName);
        }

        [HttpPost] 
        public async Task<MessageModel> RenameFile([FromBody] RenameFileDto rename)
        {

            await _fileManagerService.CheckUserDic(rename.oldFile.filePath, _user.DeptId.ObjToString());
            await _fileManagerService.CheckUserDic(rename.newFile.fileParentDic, _user.DeptId.ObjToString());
            if (rename.oldFile.fileType == "file")
            {
                string sourceFilePath = rename.oldFile.filePath;
                string destinationFilePath = Path.Combine(rename.newFile.fileParentDic, rename.newFile.fileName);
                FileHelper.FileMove(sourceFilePath, destinationFilePath);
            }
            else
            {
                string sourceDirectoryPath = rename.oldFile.filePath;
                string destinationDirectoryPath = Path.Combine(rename.newFile.fileParentDic, rename.newFile.fileName);
                Directory.Move(sourceDirectoryPath, destinationDirectoryPath);
            }
            await Task.CompletedTask;
            return MessageModel.Success("修改成功");
        }


        [HttpPost] 
        public async Task<MessageModel> MoveFile([FromBody] MoveFileDto move)
        {

            await _fileManagerService.CheckUserDic(move.toFile.filePath, _user.DeptId.ObjToString());
            await _fileManagerService.CheckUserDic(move.fromFile.filePath, _user.DeptId.ObjToString());
            if (move.fromFile.fileType == "file")
            {
                //移动文件
                var newPath = Path.Combine(move.toFile.filePath, move.fromFile.fileName);
                FileHelper.FileMove(move.fromFile.filePath, newPath);
            }
            else
            {
                //移动文件夹
                var newPath = Path.Combine(move.toFile.filePath, move.fromFile.fileName);
                FileHelper.MoveDirectory(move.fromFile.filePath, newPath);
            }
            await Task.CompletedTask;
            return MessageModel.Success("移动成功");
        }
        [HttpPost] 
        public async Task<MessageModel> CopyFile([FromBody] CopyFileDto move)
        {
            await _fileManagerService.CheckUserDic(move.toFile.filePath, _user.DeptId.ObjToString());
            await _fileManagerService.CheckUserDic(move.fromFile.filePath, _user.DeptId.ObjToString());
            if (move.fromFile.fileType == "file")
            {
                //复制文件
                var newPath = Path.Combine(move.toFile.filePath, move.fromFile.fileName);
                FileHelper.FileCoppy(move.fromFile.filePath, newPath);
            }
            else
            {
                //复制文件夹
                var newPath = Path.Combine(move.toFile.filePath, move.fromFile.fileName);
                FileHelper.CopyDir(move.fromFile.filePath, newPath);
            }
            await Task.CompletedTask;
            return MessageModel.Success("复制成功");
        }

    }
}
