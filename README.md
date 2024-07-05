Understanding the internal workings of keyset pagination and offset pagination can provide insight into why keyset pagination is generally more efficient for large datasets. Here’s a detailed explanation:

Internal Workings of Offset Pagination
Offset pagination retrieves a specific "page" of records by skipping a certain number of rows and then taking the next set of rows. Here’s how it works internally:

Counting and Skipping Rows:

The database engine counts the number of rows to skip. This is usually done by iterating through the rows until the desired offset is reached.
For example, for page 3 with a page size of 10, the query needs to skip 20 rows: SELECT * FROM Users ORDER BY Id OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY.
Fetching the Page:

After skipping the rows, the database fetches the next set of rows (the page).
Performance Issues:

As the offset increases, the database needs to skip an increasing number of rows, which can lead to slower queries.
The larger the offset, the more rows the database has to iterate through and skip, making the query less efficient.
Internal Workings of Keyset Pagination
Keyset pagination, also known as cursor-based pagination, retrieves the next set of records based on the values of the last retrieved record. Here’s how it works internally:

Using Indexed Columns:

Keyset pagination uses a unique, indexed column (e.g., Id) to determine where to start the next page of results.
For example, if the last retrieved Id is 30, the query for the next page might look like: SELECT * FROM Users WHERE Id > 30 ORDER BY Id ASC LIMIT 10.
Filtering with Indexed Values:

The database uses the index to quickly locate the starting point for the next page.
Since it uses indexed columns, it doesn’t need to count or skip rows, making the query more efficient.
Consistency:

Keyset pagination ensures consistent results even if rows are added or deleted between requests. The pagination is based on the values of the rows themselves rather than their position in the result set.
Example Queries
Offset Pagination Query
For page 3 with a page size of 10:

sql
Copy code
SELECT * FROM Users
ORDER BY Id
OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY;
OFFSET: Skips the first 20 rows.
FETCH NEXT: Fetches the next 10 rows.
Keyset Pagination Query
If the last Id of the previous page is 30:

sql
Copy code
SELECT * FROM Users
WHERE Id > 30
ORDER BY Id ASC
LIMIT 10;
WHERE Id > 30: Starts fetching rows where Id is greater than the last Id of the previous page.
ORDER BY Id ASC: Ensures the results are ordered by Id.
LIMIT: Limits the number of rows fetched to 10.
Performance Comparison
Offset Pagination
Pros: Simple to implement and understand.
Cons:
Performance degrades with higher offsets.
Requires counting and skipping rows, which becomes slower as the dataset grows.
Keyset Pagination
Pros:
More efficient for large datasets.
Uses indexed columns to quickly locate the starting point for the next page.
Ensures consistent results even with data changes.
Cons: Requires maintaining state (e.g., the last Id) between requests.
