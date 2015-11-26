# AskellaWebScraper
This is a simple webscraper. Using regex for html link extraction. 
Async and multithreading are used for downloading links and saving the files to disk.

Using:
Webscrapter.cs contains the main method. Replace "mysite" with the name of your site:
  * domainName = "mysite.com";
  * url = "http://www.mysite.com";
  * dir = @"C:\mysite\";

Known issues:
* Async and multithreading needs improvement.
* Extracting of html links is not fully recursive.
* Exception when creating folders.

