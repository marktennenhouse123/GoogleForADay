<Query Kind="Program">
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
</Query>

void Main()
{
	string testDoc = "<html><a href='stuff'> alot of a letters in a doc here</a> and regular text here </html>";
	
	string testDoc2 = "<html><p> and regular text here </p></html>";
	
	HtmlDocument doc = new HtmlDocument();
	doc.LoadHtml(testDoc);
	
	
	var hrefList = from lnk in doc.DocumentNode.SelectNodes("//a").NullGuard()
				select lnk;
				
				 if (hrefList.Any())
				 {
		
					foreach (var element in hrefList)
					{
					 var resultList =  new { Name = element.GetAttributeValue("href", "None"), LinkText = element.InnerText };
					 resultList.Dump();
					} 
				  
				}
				  //Select(p => p.GetAttributeValue("href", "not_found"))
					
								 
		//	hrefList.Dump();
}
static class ExtensionToHtmlAgility
{
	public static HtmlNodeCollection NullGuard(this HtmlNodeCollection self)
	{
		if (self == null)
			return new HtmlNodeCollection(null);
		else
			return self;
	}
}
// Define other methods and classes here
