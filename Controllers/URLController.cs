using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

namespace APKUrl.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class URLController : ControllerBase
    {
        private readonly ILogger<URLController> _logger;
        private readonly IConfiguration _configuration;

        public URLController(ILogger<URLController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }


        [HttpGet(Name = "{packageName}")]
        public string Get(string packageName, string version = "latest")
        {
            var sourceURL = $"https://d.apkpure.com/b/APK/{packageName}?version={version}";

            //var proxy = "http://homepc.local:7890";

            //IWebDriver driver = new ChromeDriver("D:\\必备工具\\chromedriver\\chromedriver.exe");
            var firefoxOptions = new ChromeOptions();
            var proxy = _configuration["Proxy"];
            if (string.IsNullOrEmpty(proxy))
                firefoxOptions.Proxy = new Proxy() {HttpProxy = proxy};
            firefoxOptions.AddArgument("--disable-web-security");

            var seleniumURL = _configuration["SeleniumURL"];

            if (!string.IsNullOrEmpty(_configuration["AUTO302_HOST"]))
            {
                seleniumURL = _configuration["AUTO302_HOST"];
            }

            using (RemoteWebDriver driver = new RemoteWebDriver(new Uri("http://"+seleniumURL), firefoxOptions))
            {


                driver.Navigate().GoToUrl("https://www.baidu.com/");

                var tools = driver as IDevTools;
                if (tools != null)
                {

                    var setStr =
                        $@"fetch(""{sourceURL}"", {{ method: 'get' }})
    .then(response => {{
        // HTTP 301 response
        // HOW CAN I FOLLOW THE HTTP REDIRECT RESPONSE?
        document.title= response.url;
        
    }})
    .catch(function(err) {{
        console.info(err + "" url: "" + url);
    }});";

                    driver.ExecuteScript(setStr);

                    WebDriverWait waitTitle = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                   waitTitle.Until(e => e.Title != "百度一下，你就知道");

                    _logger.LogInformation($"url is :{driver.Title}");
                    var title = driver.Title;

                    driver.Quit();
                    return title;

                }

                return sourceURL;

            }

        }
    }
}