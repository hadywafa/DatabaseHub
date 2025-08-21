# ⁉️ Common 30 SQL Problems

Here’s the same high-yield SQL pack rewritten **entirely in SQL Server (T-SQL) syntax**.  
Schema used in examples (for mental mapping only):

```sql
-- HR
-- Employees(emp_id PK, emp_name, dept_id, manager_id, hire_date, salary)
-- Departments(dept_id PK, dept_name)
-- Salaries(emp_id, pay_date, amount)

-- Commerce
-- Customers(cust_id PK, cust_name, city)
-- Orders(order_id PK, cust_id, order_date, status)
-- OrderItems(order_id, product_id, qty, price)
-- Products(product_id PK, product_name, category, unit_price)
-- Payments(payment_id PK, order_id, paid_on, amount)

-- Projects
-- Projects(proj_id PK, proj_name, start_date, end_date)
-- EmployeeProjects(emp_id, proj_id, assigned_on)
```

---

## A — Filtering, Sorting, Basics

**1. Top 10 newest orders:**

```sql
SELECT TOP (10) *
FROM Orders
ORDER BY order_date DESC;
```

**2. Distinct customer cities:**

```sql
SELECT DISTINCT city
FROM Customers;
```

**3. Orders in last 30 days:**

```sql
SELECT *
FROM Orders
WHERE order_date >= DATEADD(DAY, -30, CAST(GETDATE() AS date));
```

**4. Case-insensitive product search (contains 'pro'):**

```sql
SELECT *
FROM Products
WHERE LOWER(product_name) LIKE '%pro%';  -- or use COLLATE for case-insensitive if needed
```

---

## B — Aggregation, GROUP BY, HAVING

**5. Total revenue per order:**

```sql
SELECT oi.order_id,
       SUM(oi.qty * oi.price) AS order_total
FROM OrderItems AS oi
GROUP BY oi.order_id;
```

**6. Monthly revenue (last 6 months):**

```sql
WITH items AS (
  SELECT o.order_id, o.order_date, (oi.qty * oi.price) AS line_total
  FROM Orders AS o
  JOIN OrderItems AS oi ON oi.order_id = o.order_id
  WHERE o.order_date >= DATEADD(MONTH, -6, CAST(GETDATE() AS date))
)
SELECT CAST(DATEFROMPARTS(YEAR(order_date), MONTH(order_date), 1) AS date) AS [month],
       SUM(line_total) AS revenue
FROM items
GROUP BY DATEFROMPARTS(YEAR(order_date), MONTH(order_date), 1)
ORDER BY [month];
```

**7. Departments with avg salary > 15000:**

```sql
SELECT d.dept_name, AVG(e.salary) AS avg_salary
FROM Employees AS e
JOIN Departments AS d ON d.dept_id = e.dept_id
GROUP BY d.dept_name
HAVING AVG(e.salary) > 15000;
```

---

## C — JOINs (inner, left, anti)

**8. Customers who never placed an order (anti-join):**

```sql
SELECT c.*
FROM Customers AS c
LEFT JOIN Orders AS o ON o.cust_id = c.cust_id
WHERE o.order_id IS NULL;
```

**9. Orders with payment status (paid vs total):**

```sql
WITH order_totals AS (
  SELECT oi.order_id, SUM(oi.qty * oi.price) AS total
  FROM OrderItems AS oi
  GROUP BY oi.order_id
),
paid AS (
  SELECT p.order_id, SUM(p.amount) AS paid_amount
  FROM Payments AS p
  GROUP BY p.order_id
)
SELECT o.order_id, o.order_date,
       ot.total,
       ISNULL(p.paid_amount, 0) AS paid_amount,
       CASE WHEN ISNULL(p.paid_amount, 0) >= ot.total THEN 'PAID' ELSE 'DUE' END AS pay_status
FROM Orders AS o
JOIN order_totals AS ot ON ot.order_id = o.order_id
LEFT JOIN paid AS p ON p.order_id = o.order_id;
```

**10. Employees with their manager names (self-join):**

```sql
SELECT e.emp_name AS employee,
       m.emp_name AS manager
FROM Employees AS e
LEFT JOIN Employees AS m ON m.emp_id = e.manager_id;
```

---

## D — Window Functions (rank, running totals, top-N per group)

**11. 2nd highest salary:**

```sql
SELECT emp_id, emp_name, salary
FROM (
  SELECT e.*,
         DENSE_RANK() OVER (ORDER BY salary DESC) AS rk
  FROM Employees AS e
) AS s
WHERE rk = 2;
```

**12. Top 3 earners per department:**

```sql
SELECT dept_name, emp_name, salary
FROM (
  SELECT d.dept_name, e.emp_name, e.salary,
         DENSE_RANK() OVER (PARTITION BY d.dept_name ORDER BY e.salary DESC) AS rnk
  FROM Employees AS e
  JOIN Departments AS d ON d.dept_id = e.dept_id
) AS t
WHERE rnk <= 3
ORDER BY dept_name, rnk, salary DESC;
```

**13. Running revenue per customer by order date:**

```sql
WITH cust_orders AS (
  SELECT o.cust_id, o.order_id, o.order_date,
         SUM(oi.qty * oi.price) AS order_total
  FROM Orders AS o
  JOIN OrderItems AS oi ON oi.order_id = o.order_id
  GROUP BY o.cust_id, o.order_id, o.order_date
)
SELECT c.cust_id, c.order_id, c.order_date, c.order_total,
       SUM(c.order_total) OVER (PARTITION BY c.cust_id ORDER BY c.order_date
                                ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS running_total
FROM cust_orders AS c
ORDER BY c.cust_id, c.order_date;
```

**14. Latest salary payment per employee:**

```sql
SELECT emp_id, pay_date, amount
FROM (
  SELECT s.*,
         ROW_NUMBER() OVER (PARTITION BY emp_id ORDER BY pay_date DESC) AS rn
  FROM Salaries AS s
) AS x
WHERE rn = 1;
```

---

## E — Subqueries & CTEs

**15. Employees earning above their department’s average:**

```sql
SELECT e.*
FROM Employees AS e
JOIN (
  SELECT dept_id, AVG(salary) AS avg_salary
  FROM Employees
  GROUP BY dept_id
) AS a
  ON a.dept_id = e.dept_id
WHERE e.salary > a.avg_salary;
```

**16. Orders above global average order value:**

```sql
WITH totals AS (
  SELECT order_id, SUM(qty * price) AS total
  FROM OrderItems
  GROUP BY order_id
),
avg_total AS (
  SELECT AVG(total) AS avg_total FROM totals
)
SELECT t.*
FROM totals AS t
CROSS JOIN avg_total AS a
WHERE t.total > a.avg_total;
```

**17. Customers who bought in **every** month of Q2-2025 (Apr-Jun):**

```sql
WITH months AS (
  SELECT CAST('2025-04-01' AS date) AS m UNION ALL
  SELECT CAST('2025-05-01' AS date) UNION ALL
  SELECT CAST('2025-06-01' AS date)
),
cust_months AS (
  SELECT DISTINCT o.cust_id,
         DATEFROMPARTS(YEAR(o.order_date), MONTH(o.order_date), 1) AS m
  FROM Orders AS o
  WHERE o.order_date >= '2025-04-01' AND o.order_date < '2025-07-01'
),
counts AS (
  SELECT cust_id, COUNT(*) AS cnt
  FROM cust_months
  WHERE m IN (SELECT m FROM months)
  GROUP BY cust_id
)
SELECT c.cust_id
FROM counts AS c
WHERE c.cnt = (SELECT COUNT(*) FROM months);
```

---

## F — “Nth highest”, duplicates, de-duplication

**18. Nth highest salary (parameterized):**

```sql
-- Set @N before running
-- DECLARE @N int = 2;

SELECT emp_id, emp_name, salary
FROM (
  SELECT e.*,
         DENSE_RANK() OVER (ORDER BY salary DESC) AS rk
  FROM Employees AS e
) AS s
WHERE rk = @N;
```

**19. Find duplicate customers by (cust_name, city):**

```sql
SELECT cust_name, city, COUNT(*) AS cnt
FROM Customers
GROUP BY cust_name, city
HAVING COUNT(*) > 1;
```

**20. Delete duplicates (keep the smallest cust_id):**

```sql
WITH dups AS (
  SELECT *,
         ROW_NUMBER() OVER (PARTITION BY cust_name, city ORDER BY cust_id) AS rn
  FROM Customers
)
DELETE FROM dups
WHERE rn > 1;
```

---

## G — Date/time & conditional aggregation

**21. Orders per status in last 90 days (pivoted counts):**

```sql
SELECT
  SUM(CASE WHEN status = 'NEW'       THEN 1 ELSE 0 END) AS new_cnt,
  SUM(CASE WHEN status = 'SHIPPED'   THEN 1 ELSE 0 END) AS shipped_cnt,
  SUM(CASE WHEN status = 'CANCELLED' THEN 1 ELSE 0 END) AS cancelled_cnt
FROM Orders
WHERE order_date >= DATEADD(DAY, -90, CAST(GETDATE() AS date));
```

**22. Average days from order to first payment:**

```sql
WITH order_total AS (
  SELECT o.order_id, o.order_date, SUM(oi.qty*oi.price) AS total
  FROM Orders AS o
  JOIN OrderItems AS oi ON oi.order_id = o.order_id
  GROUP BY o.order_id, o.order_date
),
first_payment AS (
  SELECT p.order_id, MIN(CAST(p.paid_on AS date)) AS first_paid_on
  FROM Payments AS p
  GROUP BY p.order_id
)
SELECT AVG(DATEDIFF(DAY, ot.order_date, fp.first_paid_on)) AS avg_days_to_first_payment
FROM order_total AS ot
JOIN first_payment AS fp ON fp.order_id = ot.order_id;
```

---

## H — String ops & CASE

**23. Normalize product category display (fallback if NULL/blank):**

```sql
SELECT product_id, product_name,
       CASE
         WHEN category IS NULL OR LTRIM(RTRIM(category)) = '' THEN 'Uncategorized'
         ELSE category  -- (Proper-casing is non-trivial in T-SQL; keep as-is)
       END AS category_display
FROM Products;
```

**24. Mask customer names (first letter + asterisks):**

```sql
SELECT cust_id,
       LEFT(cust_name, 1) + REPLICATE('*', CASE WHEN LEN(cust_name) - 1 < 0 THEN 0 ELSE LEN(cust_name) - 1 END) AS masked_name
FROM Customers;
```

---

## I — Top-N per group variations

**25. Most recent order per customer:**

```sql
SELECT cust_id, order_id, order_date
FROM (
  SELECT o.*,
         ROW_NUMBER() OVER (PARTITION BY cust_id ORDER BY order_date DESC) AS rn
  FROM Orders AS o
) AS x
WHERE rn = 1;
```

**26. Best-selling product per category (by revenue):**

```sql
WITH prod_rev AS (
  SELECT p.category, p.product_id, p.product_name,
         SUM(oi.qty * oi.price) AS revenue
  FROM Products AS p
  JOIN OrderItems AS oi ON oi.product_id = p.product_id
  GROUP BY p.category, p.product_id, p.product_name
),
ranked AS (
  SELECT pr.*,
         ROW_NUMBER() OVER (PARTITION BY pr.category ORDER BY pr.revenue DESC) AS rn
  FROM prod_rev AS pr
)
SELECT category, product_id, product_name, revenue
FROM ranked
WHERE rn = 1;
```

---

## J — Gaps & Islands (streaks)

**27. Consecutive payment days per order (islands):**

```sql
-- Build groups by subtracting an increasing day offset
WITH d AS (
  SELECT DISTINCT order_id, CAST(paid_on AS date) AS dte
  FROM Payments
),
x AS (
  SELECT order_id, dte,
         DATEADD(DAY, -ROW_NUMBER() OVER (PARTITION BY order_id ORDER BY dte), dte) AS grp_key
  FROM d
)
SELECT order_id,
       MIN(dte) AS streak_start,
       MAX(dte) AS streak_end,
       COUNT(*)  AS days
FROM x
GROUP BY order_id, grp_key
ORDER BY order_id, streak_start;
```

---

## K — Recursive CTEs (hierarchies)

**28. Manager → subordinates tree for a given @manager_id:**

```sql
-- DECLARE @manager_id int = 101;

WITH org AS (
  SELECT emp_id, emp_name, manager_id, 0 AS lvl
  FROM Employees
  WHERE emp_id = @manager_id
  UNION ALL
  SELECT e.emp_id, e.emp_name, e.manager_id, o.lvl + 1
  FROM Employees AS e
  JOIN org AS o ON o.emp_id = e.manager_id
)
SELECT *
FROM org
ORDER BY lvl, emp_name
OPTION (MAXRECURSION 32767);
```

---

## L — Set operations

**29. Customers who bought “Laptops” but never “Accessories”:**

```sql
WITH buyers AS (
  SELECT DISTINCT o.cust_id, p.category
  FROM Orders AS o
  JOIN OrderItems AS oi ON oi.order_id = o.order_id
  JOIN Products AS p ON p.product_id = oi.product_id
)
SELECT cust_id
FROM buyers
WHERE category = 'Laptops'
EXCEPT
SELECT cust_id
FROM buyers
WHERE category = 'Accessories';
```

---

## M — Updates & Deletes with joins

**30. Increase salary by 5% for employees on >2 active projects:**

```sql
WITH counts AS (
  SELECT ep.emp_id, COUNT(*) AS c
  FROM EmployeeProjects AS ep
  JOIN Projects AS pr ON pr.proj_id = ep.proj_id
  WHERE pr.end_date IS NULL OR pr.end_date > CAST(GETDATE() AS date)
  GROUP BY ep.emp_id
)
UPDATE e
SET e.salary = e.salary * 1.05
FROM Employees AS e
JOIN counts AS c ON c.emp_id = e.emp_id
WHERE c.c > 2;
```

---

## Quick exam tips (SQL Server specific)

- **Date math:** `DATEADD`, `DATEDIFF`, `EOMONTH`, `DATEFROMPARTS`.
- **NULL handling:** prefer `ISNULL()` (or `COALESCE()`), watch out in arithmetic.
- **Top-N:** Windows (`ROW_NUMBER`, `DENSE_RANK`) beat correlated subqueries for clarity.
- **Anti-joins:** `LEFT JOIN ... IS NULL` or `NOT EXISTS`.
- **De-dupe deletes:** `ROW_NUMBER()` in a CTE then `DELETE`.

If you want, I can package these as a **runnable .sql practice file** (with table DDL + seed data + questions).
