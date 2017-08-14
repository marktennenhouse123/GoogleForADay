<Query Kind="Statements">
  <Connection>
    <ID>6fc9eb97-b4d4-45a9-b7f7-e31b66499617</ID>
    <Persist>true</Persist>
    <Server>ARK-FL-MWS-01\LENOVODB</Server>
    <SqlSecurity>true</SqlSecurity>
    <UserName>testuser</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAnYLfVW8DM06CbEpq5ZDx9AAAAAACAAAAAAADZgAAwAAAABAAAAB9TxqxYuGlufu8jcoTNf4aAAAAAASAAACgAAAAEAAAACOYKHzpsinVDCiHJXs1jUcQAAAAFI2wZCq4uO73pf30P7KDTBQAAAAPYZu6Ut+/pH8RWbmRBzIRu+EWQg==</Password>
    <Database>CloudDB</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

string keyword= "is";

var results = (from pg in IndexedPages
			 join pgLinks in PageKeyWords
			 on pg.PageID equals pgLinks.PageID
			 
			   where pgLinks.Keyword.Contains(keyword) || pgLinks.Keyword.StartsWith(keyword)
			  
			    select new 
				   {
					   PageName = pg.PageName,
					   Keyword = pgLinks.Keyword,
					   Rank = pgLinks.KeywordCount.Value,
					   PageURL = pg.PageURL,
					   Title = pg.Title
				   }).ToList();
				   

results.Dump();
 var g2 = from pgs in results
		  group pgs by pgs.PageURL into grup1
		  select new { myPage = grup1.Key, Total = grup1.Sum(g => g.Rank)};
 g2.Dump();