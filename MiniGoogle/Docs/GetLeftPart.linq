<Query Kind="Statements">
  <Connection>
    <ID>545727dc-8880-411b-9ef5-9cb554ba2b8a</ID>
    <Persist>true</Persist>
    <Server>CloudserverMain.Database.windows.net</Server>
    <SqlSecurity>true</SqlSecurity>
    <UserName>testuser</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAnYLfVW8DM06CbEpq5ZDx9AAAAAACAAAAAAADZgAAwAAAABAAAAB7xUkQB4XtVceFFBSRjUf1AAAAAASAAACgAAAAEAAAAFuW7yKKa6FOhLf8rYRrqXMQAAAANme/ocAuVVhRnlzht7eBbhQAAADaHqiwURKraBDQKLLQYANTO7s09A==</Password>
    <DbVersion>Azure</DbVersion>
    <Database>CloudDB</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

string pageURL ="http:www.pizzarox.com/#menu";
string pageName = "#menu";
 Uri siteURL = new Uri(pageURL);
 string domain = siteURL.GetLeftPart(UriPartial.Authority);
var results= (from p in IndexedPages
 where p.PageName.ToLower() == pageName.ToLower()
&& p.PageURL.StartsWith(domain)
 select p).ToList();
 results.Dump();