# ACT.ScreenGrab
Uses selenium with chrome driver to take full page screenshots from a provided csv file.
optionally reduces content to just a specific set of elements if the page supports jquery.


Just run
ACT.ScreenGrab.ConsoleApp.exe

it will provide you with a pages.csv file which is a csv of "url, jQuerySelector"

outputs screenshots to ./screenshots/URL_{index}.png