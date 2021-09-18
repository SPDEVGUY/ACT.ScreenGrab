using System;
using System.Text;
using OpenQA.Selenium;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

using Microsoft.VisualBasic.FileIO;

namespace ACT.ScreenGrab.ConsoleApp
{
    class Program
    {
        public static readonly string ScreenshotDirectory = Environment.CurrentDirectory + @"\screenshots\";
        public const ScreenshotImageFormat OutputFormat = ScreenshotImageFormat.Png;

        public static readonly string UrlListFile = "pages.csv";

        static void Main(string[] args)
        {
            var urlList = Path.Combine(Environment.CurrentDirectory, UrlListFile);
            if (!File.Exists(urlList))
            {
                Console.WriteLine($"Didn't find {urlList}. Creating default...");
                File.WriteAllText(urlList,
                    "\"URL\",\"jQuery Content Selector (Optional)\"" + Environment.NewLine
                    + "\"https://www.google.ca\"" + Environment.NewLine
                    + "\"https://stackoverflow.com/questions/3422262/how-can-i-take-a-screenshot-with-selenium-webdriver\",\"#content\"",
                    Encoding.UTF8
                    );
                Process.Start(urlList);
            } else
            {
                using (var driver = new OpenQA.Selenium.Chrome.ChromeDriver())
                {
                    try
                    {
                        var n = driver.Navigate();

                        using (var csvParser = new TextFieldParser(urlList))
                        {
                            csvParser.CommentTokens = new string[] { "#" };
                            csvParser.SetDelimiters(new string[] { "," });
                            csvParser.HasFieldsEnclosedInQuotes = true;

                            // Skip the row with the column names
                            csvParser.ReadLine();

                            var urlIndex = 0;
                            while (!csvParser.EndOfData)
                            {
                                // Read current line fields, pointer moves to the next line.
                                string[] fields = csvParser.ReadFields();
                                var url = fields[0];
                                var selector = fields.Length > 1 ? fields[1] : null;

                                n.GoToUrl(url);
                                var fileName = $"URL_{urlIndex}.png";
                                TakeSnapshot(driver, fileName, selector);
                                urlIndex++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        driver.Quit();
                    }
                }
            }
        }



        /// <summary>
        /// Take a screenshot of the whole page by merging the results of scrolling together.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="fileName"></param>
        /// <param name="scrollContainerId"></param>
        public static void TakeSnapshot(IWebDriver driver, string fileName, string scrollContainerId = null)
        {
            //Maximize the resolution
            driver.Manage().Window.FullScreen();

            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            var fileLocation = Path.Combine(ScreenshotDirectory, fileName);
            if (!Directory.Exists(ScreenshotDirectory))
            {
                Directory.CreateDirectory(ScreenshotDirectory);
            }

            // get the full page height and current browser height
            if (string.IsNullOrEmpty(scrollContainerId))
            {
                js.ExecuteScript(@"
                window.browserHeight = (window.innerHeight || document.body.clientHeight);
                window.fullPageHeight = document.body.scrollHeight;
            ");
            } else
            {
                js.ExecuteScript(@"
                    var contentId = '" + scrollContainerId + @"';
                    if($) {
                        var exclude = $(contentId).add($(contentId).parents()).add($(contentId).find('DIV'))
                        $('DIV').not(exclude).remove();
                        $('HEADER').not(exclude).remove();
                        $('FOOTER').not(exclude).remove();
                        $(contentId).parents().css({'margin-left':0,'margin-top':0,'margin-right':0,'margin-bottom':0,'padding-top':0,'padding-left':0,'padding-right':0,'padding-bottom':0});
                    }
                    window.browserHeight = (window.innerHeight || document.body.clientHeight);
                    window.fullPageHeight = document.body.scrollHeight;
                ");
            }
            var windowHeight = Convert.ToInt32(js.ExecuteScript(@"return window.browserHeight;"));
            var fullPageHeight = Convert.ToInt32(js.ExecuteScript(@"return window.fullPageHeight"));

            if (windowHeight == fullPageHeight)
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(fileLocation, OutputFormat);
            }
            else
            {
                var scrollsRequired = fullPageHeight / windowHeight;
                var remainderHeight = (fullPageHeight) % windowHeight;
                var scollCount = (remainderHeight > 0 || scrollsRequired == 0) ? scrollsRequired + 1 : scrollsRequired;

                // back to top start screenshot
                js.ExecuteScript("window.scrollTo(0, 0)");

                var imageBaseContent = ((ITakesScreenshot)driver).GetScreenshot().AsByteArray;
                
                var finalImage = BitmapFromByteArray(imageBaseContent);

                for (int count = 1; count < scollCount; count++)
                {
                    js.ExecuteScript(@"window.scrollBy(0, window.browserHeight);");
                    //Thread.Sleep(10);
                    Byte[] imageContentAdd = ((ITakesScreenshot)driver).GetScreenshot().AsByteArray;

                    var source = BitmapFromByteArray(imageContentAdd);

                    // cut the last screen shot if last screesshot override with sencond last one
                    if (count == (scollCount - 1))
                    {
                        Bitmap trimmedTailImage =
                            source.Clone(new Rectangle(0, windowHeight - remainderHeight, source.Width, remainderHeight), source.PixelFormat);

                        var combined =  CombineImages(finalImage, trimmedTailImage);
                        finalImage.Dispose();
                        trimmedTailImage.Dispose();

                        finalImage = combined;
                    }
                    //cut the site header from screenshot
                    else
                    {
                        var combined = CombineImages(finalImage, source);

                        finalImage.Dispose();

                        finalImage = combined;
                    }

                    source.Dispose();
                }

                if (!Directory.Exists(ScreenshotDirectory))
                {
                    Directory.CreateDirectory(ScreenshotDirectory);
                }
                finalImage.Save(fileLocation, ImageFormat.Png);
                finalImage.Dispose();
            }
        }

        //combine two pictures
        public static Bitmap CombineImages(Image image1, Image image2)
        {
            Bitmap bitmap = new Bitmap(image1.Width, image1.Height + image2.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(image1, 0, 0);
                g.DrawImage(image2, 0, image1.Height);
            }

            return bitmap;
        }

        public static Bitmap BitmapFromByteArray(byte[] input)
        {
            Image imageAdd;
            
            using (var msAdd = new MemoryStream(input))
            {
                imageAdd = Image.FromStream(msAdd);

                var result = new Bitmap(width:imageAdd.Width,height:imageAdd.Height);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(imageAdd, 0, 0);
                }

                return result;
            }
        }
    }
}
