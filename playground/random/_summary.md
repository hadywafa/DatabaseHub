# 💡 **Summary**

1. Review WINDOW FUNCTION
2. YEAR()
3. GROUP BY .. HAVING \<condition>

## RANK 😡 Hates WHERE Clause

```sql
SELECT emp_id, emp_name, salary,
       RANK() OVER (ORDER BY salary) AS rank
FROM Employee
WHERE rank = 2;
```

❌ Error: _Invalid column name 'rank'_

- `WHERE` is evaluated **before** the window function (`RANK()`) is computed.
- You can’t directly filter on the alias inside the same query.
