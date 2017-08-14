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

let sum = grup1.Sum(x => x.KeywordCount)
		 let count = grup1.Count()
		 select new { mySum = sum, myCount = count }).ToList();



(from p in IndexedPages
 join pkw in PageKeyWords
 on p.PageID equals pkw.PageID
 group p by p.PageName into gp
 
 
 var pgCount = (from px in IndexedPages
		 group px by px.PageURL into gr1
		 select new { myKey = gr1.Key, mycount = gr1.Count()}).ToList();
		pgCount.Sum(g =>g.mycount ).Dump();

var kwCount = (from p in IndexedPages
			  join pkw in PageKeyWords
			  on p.PageID equals pkw.PageID
			  where p.IndexedSiteID == 33
			  group p by p.PageName into gp
			  select new {myKey = gp.Key, myKWCount = gp.Count()}).ToList();
	kwCount.Sum(c =>c.myKWCount ).Dump();
	
	
	
	