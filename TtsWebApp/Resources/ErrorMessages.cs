using System.Globalization;
using System.Reflection;

namespace TtsWebApp.Resources
{
    public static class ErrorMessages
    {
        private static readonly CultureInfo _culture = CultureInfo.CurrentUICulture;
        
        public static string SaveConversionRecordFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "保存转换记录失败" : "Failed to save conversion record";
            
        public static string ApiCallFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "API调用失败" : "API call failed";
            
        public static string SaveFailureRecordError => 
            _culture.TwoLetterISOLanguageName == "zh" ? "保存失败记录时出错" : "Error saving failure record";
            
        public static string ConversionError => 
            _culture.TwoLetterISOLanguageName == "zh" ? "转换错误: {0}" : "Conversion error: {0}";
            
        public static string UnsafePath => 
            _culture.TwoLetterISOLanguageName == "zh" ? "路径不安全" : "Unsafe path";
            
        public static string FilePathCannotBeEmpty => 
            _culture.TwoLetterISOLanguageName == "zh" ? "文件路径不能为空" : "File path cannot be empty";
            
        public static string BrowseFileFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "浏览文件失败" : "Failed to browse files";
            
        public static string OpenFileFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "打开文件失败" : "Failed to open file";
            
        public static string CannotEditFile => 
            _culture.TwoLetterISOLanguageName == "zh" ? "无法编辑此文件" : "Cannot edit this file";
            
        public static string SaveFileFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "保存文件失败" : "Failed to save file";
            
        public static string UploadFileFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "上传文件失败" : "Failed to upload file";
            
        public static string CreateFolderFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "创建文件夹失败" : "Failed to create folder";
            
        public static string PathCannotBeEmpty => 
            _culture.TwoLetterISOLanguageName == "zh" ? "路径不能为空" : "Path cannot be empty";
            
        public static string DeleteFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "删除失败" : "Delete failed";
            
        public static string ParametersCannotBeEmpty => 
            _culture.TwoLetterISOLanguageName == "zh" ? "参数不能为空" : "Parameters cannot be empty";
            
        public static string RenameFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "重命名失败" : "Rename failed";
            
        public static string FileDoesNotExist => 
            _culture.TwoLetterISOLanguageName == "zh" ? "文件不存在" : "File does not exist";
            
        public static string DownloadFileFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "下载文件失败" : "Failed to download file";
            
        public static string GetFileContentFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "获取文件内容失败" : "Failed to get file content";
            
        public static string PreviewImageFailed => 
            _culture.TwoLetterISOLanguageName == "zh" ? "预览图片失败" : "Failed to preview image";
            
        public static string SettingsSavedSuccessfully => 
            _culture.TwoLetterISOLanguageName == "zh" ? "设置保存成功" : "Settings saved successfully";
    }
}