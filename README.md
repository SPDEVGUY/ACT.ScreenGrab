# ACT.ScreenGrab
Uses selenium with chrome driver to take full page screenshots from a provided csv file.
optionally reduces content to just a specific set of elements if the page supports jquery.

Made this tool so I can quickly analyze a whole bunch of sites and pages as part of a rebranding and upgrade project. 
Needed a way to quickly review all the sites and automatically look for any errors.


Just run
ACT.ScreenGrab.ConsoleApp.exe [DriverName]
ACT.ScreenGrab.ConsoleApp.exe Firefox
ACT.ScreenGrab.ConsoleApp.exe InternetExplorer

DriverName is one of the following:
	Chrome (default)
    Firefox
    InternetExplorer
    Opera


Notes on driver urls:

FireFox/GeckoDriver - https://github.com/mozilla/geckodriver/releases
 https://github.com/mozilla/geckodriver/releases/download/v0.30.0/geckodriver-v0.30.0-win64.zip

ChromeDriver - https://chromedriver.chromium.org/downloads
 https://chromedriver.storage.googleapis.com/index.html?path=94.0.4606.41/ - alpha channel (2021-09-18)
 https://chromedriver.storage.googleapis.com/index.html?path=93.0.4577.63/ - Release (get this one 2021-09-18)
 
InternetExplorer - https://www.selenium.dev/downloads/
 https://github.com/SeleniumHQ/selenium/releases/download/selenium-3.150.0/IEDriverServer_x64_3.150.2.zip
 https://github.com/SeleniumHQ/selenium/wiki/InternetExplorerDriver#required-configuration


it will provide you with a pages.csv file which is a csv of "url, jQuerySelector"

outputs screenshots to ./screenshots/[DriverName]/URL_{index}.png
outputs javascript console errors to ./screenshots/[DriverName]/URL_{index}_logs.csv

Future ideas:
- Interactive logon cookie setup (for sharepoint online and oath type things...)
- Link / Src scanning  dump out referenced urls maybe for link checking, I already have another project for that.

Expand into a project where testers can quickly review look and feel differences between two sites for migration testing.
