using System.IO;
using System.Text;

namespace ServerCore.Helpers
{
    public static class NewFileCreationHelper
    {
        public enum ContentType {
            GlobalCss = 0,
            HomeContent,
            FAQContent,
            RulesContent,
        }

        public static async void CreateNewEventFiles(int eventId, string sharedResourceDirectoryName) {
            try
            {
                await FileManager.UploadBlobAsync("global-styles.css", eventId, NewFileCreationHelper.GetStreamContent(NewFileCreationHelper.ContentType.GlobalCss), sharedResourceDirectoryName);
                await FileManager.UploadBlobAsync("home-content.html", eventId, NewFileCreationHelper.GetStreamContent(NewFileCreationHelper.ContentType.HomeContent), sharedResourceDirectoryName);
                await FileManager.UploadBlobAsync("faq-content.html", eventId, NewFileCreationHelper.GetStreamContent(NewFileCreationHelper.ContentType.FAQContent), sharedResourceDirectoryName);
                await FileManager.UploadBlobAsync("rules-content.html", eventId, NewFileCreationHelper.GetStreamContent(NewFileCreationHelper.ContentType.RulesContent), sharedResourceDirectoryName);
            }
            catch (System.Exception)
            {
                // If this fails to create the files, we can ignore it and continue
            }
        }

        public static Stream GetStreamContent(ContentType contentType) {
            MemoryStream streamContent = new();
            switch (contentType)
            {
                case ContentType.GlobalCss:
                    streamContent.Write(Encoding.UTF8.GetBytes(globalCssContent));
                    break;
                case ContentType.HomeContent:
                    streamContent.Write(Encoding.UTF8.GetBytes(homeContentContent));
                    break;
                case ContentType.FAQContent:
                    streamContent.Write(Encoding.UTF8.GetBytes(faqContentContent));
                    break;
                case ContentType.RulesContent:
                    streamContent.Write(Encoding.UTF8.GetBytes(rulesContentContent));
                    break;
                default:
                    break;
            }
            streamContent.Seek(0, SeekOrigin.Begin);
            return streamContent;
        }

        private static readonly string globalCssContent = ":root {\r\n    --microsoft-yellow: rgb(255,185,0);\r\n    --microsoft-red: rgb(242, 80, 34);\r\n    --microsoft-blue: rgb(0, 164, 239);\r\n    --microsoft-green: rgb(127, 186, 0);\r\n}\r\n";
        private static readonly string homeContentContent = "<!DOCTYPE html>\r\n<html lang=\"en-us\">\r\n<head>\r\n    <title>Homepage Content</title>\r\n    <script type=\"text/javascript\" src=\"https://puzzlehunt.azurewebsites.net/js/embed-resize.js\"></script>\r\n    <link rel=\"stylesheet\" href=\"https://puzzlehunt.azurewebsites.net/css/site.min.css\">\r\n    <style>\r\n        body {\r\n            overflow-y: hidden;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <h3>Homepage Content Goes Here</h3>\r\n</body>\r\n</html>\r\n";
        private static readonly string faqContentContent = "<!DOCTYPE html>\r\n<html lang=\"en-us\">\r\n<head>\r\n    <title>FAQ Content</title>\r\n    <script type=\"text/javascript\" src=\"https://puzzlehunt.azurewebsites.net/js/embed-resize.js\"></script>\r\n    <link rel=\"stylesheet\" href=\"https://puzzlehunt.azurewebsites.net/css/site.min.css\">\r\n    <style>\r\n        body {\r\n            overflow-y: hidden;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <h3>FAQ Content Goes Here</h3>\r\n</body>\r\n</html>\r\n";
        private static readonly string rulesContentContent = "<!DOCTYPE html>\r\n<html lang=\"en-us\">\r\n<head>\r\n    <title>Rules Content</title>\r\n    <script type=\"text/javascript\" src=\"https://puzzlehunt.azurewebsites.net/js/embed-resize.js\"></script>\r\n    <link rel=\"stylesheet\" href=\"https://puzzlehunt.azurewebsites.net/css/site.min.css\">\r\n    <style>\r\n        body {\r\n            overflow-y: hidden;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <h3>Rules Content Goes Here</h3>\r\n</body>\r\n</html>\r\n";
    }
}
