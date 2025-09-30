# 📑 Solutions

<details>
<summary class="header">✅ Employee & Salary Related</summary>

## 📌 **1. Second highest salary**

### ❌ Wrong Approaches

```sql
SELECT emp_id, emp_name, salary,
       RANK() OVER (ORDER BY salary) AS rank
FROM Employee
WHERE rank = 2;
```

> Error: _Invalid column name 'rank'_
>
> - `WHERE` is evaluated **before** the window function (`RANK()`) is computed.
> - You can’t directly filter on the alias inside the same query.

---

### ✅ Correct Approaches

#### **Option 1: Subquery / CTE**

Wrap it in a subquery so you can filter on the computed rank:

```sql
WITH Ranked AS (
    SELECT
        emp_id, emp_name, salary,
        RANK() OVER (ORDER BY salary DESC) AS rnk
    FROM Employees
)
SELECT emp_id, emp_name, salary
FROM Ranked
WHERE rnk = 2;
```

If salaries can tie, and you want the “second highest” regardless of duplicates:

```sql
WITH Ranked AS (
    SELECT
        emp_id, emp_name, salary,
        DENSE_RANK() OVER (ORDER BY salary DESC) AS dr
    FROM Employees
)
SELECT *
FROM Ranked
WHERE dr = 2;
```

- Use `ORDER BY salary DESC` so **highest salary = rank 1, second highest = rank 2**.

---

#### **Option 2: Order with OFFSET**

Classic approach without OFFSET:

```sql
-- ✅ Done
SELECT salary
FROM Employees
ORDER BY salary DESC
OFFSET 1 ROW FETCH NEXT 1 ROW ONLY;
```

---

#### **Option 3: `DENSE_RANK()`**

If salaries can tie, and you want the “second highest” regardless of duplicates:

```sql
WITH Ranked AS (
    SELECT
        emp_id, emp_name, salary,
        DENSE_RANK() OVER (ORDER BY salary DESC) AS dr
    FROM Employees
)
SELECT *
FROM Ranked
WHERE dr = 2;
```

---

## 📌 **2. Top 3 highest-paid employees**

### ❌ Wrong Approaches

```sql
SELECT TOP 3 emp_name, salary
FROM Employees
ORDER BY salary DESC
```

```sql
SELECT emp_name, salary
FROM Employees
ORDER BY salary DESC
OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY;
```

> there is may be another employees with same 3 highest-paid
>
> - Returns exactly 3 rows.
> - If two or more employees tie on the 3rd salary, only 3 rows will be shown — ties are cut off.

---

### ✅ Correct Approaches

```sql
WITH Ranked AS (
    SELECT
        emp_name,
        salary,
        DENSE_RANK() OVER (ORDER BY salary DESC) AS rnk
    FROM Employees
)
SELECT emp_name, salary
FROM Ranked
WHERE rnk <= 3
ORDER BY salary DESC;

```

---

## 📌 **3. Employees earning above company average**

### ❌ Wrong Approaches

```sql
SELECT *,
AVG(salary) over() as avg_salary
FROM Employees
WHERE salary > avg_salary
```

> you can’t reference a column alias (avg_salary) directly in the WHERE clause when it's derived from a window function.

---

### ✅ Correct Approaches

#### **Option 1:**

```sql
SELECT emp_name, salary
FROM Employees
WHERE salary > (
    SELECT AVG(salary) FROM Employees
)
ORDER BY salary DESC;
```

#### **Option 2:**

```sql
WITH SalaryWithAvg AS (
  SELECT emp_name, salary,
         AVG(salary) OVER () AS avg_salary
  FROM Employees
)
SELECT emp_name, salary
FROM SalaryWithAvg
WHERE salary > avg_salary
ORDER BY salary DESC;
```

---

## 📌 **4. Number of employees in each department**

### ✅ Correct Approaches

```sql
SELECT d.dept_name, COUNT(*) AS emp_count
FROM Departments d
LEFT JOIN Employees e ON e.dept_id = d.dept_id
GROUP BY d.dept_name
ORDER BY d.dept_name;
```

---

## 📌 **5. Departments with avg salary > 10,000**

### ✅ Correct Approaches

```sql
SELECT d.dept_name, AVG(e.salary) AS avg_salary
FROM Employees e
JOIN Departments d ON d.dept_id = e.dept_id
GROUP BY d.dept_name
HAVING AVG(e.salary) > 10000;
```

---

## 📌 **6. Employees without a department**

### ✅ Correct Approaches

```sql
SELECT *
FROM Employees
WHERE dept_id IS NULL;
```

---

## 📌 **7. Employees who joined in 2022**

### ✅ Correct Approaches

```sql
SELECT *
FROM Employees
WHERE YEAR(hire_date) = 2022;
```

---

## 📌 **8. Employees sharing the same salary with someone else**

### ✅ Correct Approaches

#### **Option 1:**

```sql
WITH same_salary (
SELECT
    emp_id, emp_name, COUNT(*) OVER(PARITION BY salary) as same_salary_count
FROM Employees e
)
SELECT emp_id, emp_name
FROM same_salary
WHERE same_salary_count > 1
```

#### **Option 2:**

```sql
SELECT e.emp_id, e.emp_name
FROM Employees e
JOIN (
  SELECT salary
  FROM Employees
  GROUP BY salary
  HAVING COUNT(*) > 1
) s ON s.salary = e.salary
ORDER BY e.salary DESC, e.emp_name;
```

#### **Option 3:**

```sql
-- ✅ Done
SELECT emp_id, emp_name, salary
FROM Employees
WHERE salary IN (
    SELECT salary
    FROM Employees
    GROUP BY salary
    HAVING COUNT(*) > 1
)
ORDER BY salary DESC, emp_name;
```

---

## 📌 **9. Highest-paid employee in each department**

### ❌ Wrong Approaches

```sql
SELECT e.*, MAX(salary) AS max_salary
FROM Employee e
JOIN Department d ON d.dept_id = e.dept_id
GROUP BY dept_id
```

> SQL requires that every selected column either:
>
> - Appears in the GROUP BY clause, or
> - Is wrapped in an aggregate function (like MAX, SUM, etc.)

---

### ✅ Correct Approaches

```sql
WITH ranked_by_dept AS (
  SELECT d.dept_name, e.emp_name, e.salary,
         ROW_NUMBER() OVER (PARTITION BY d.dept_name ORDER BY e.salary DESC, e.emp_name) AS rnk
  FROM Employees e
  JOIN Departments d ON d.dept_id = e.dept_id
)
SELECT dept_name, emp_name, salary
FROM ranked_by_dept
WHERE rnk = 1
ORDER BY dept_name;
```

---

## 📌 **10. Total salary expense per department**

### ✅ Correct Approaches

```sql
SELECT d.dept_name, SUM(e.salary) AS total_salary
FROM Employees e
JOIN Departments d ON d.dept_id = e.dept_id
GROUP BY d.dept_name
ORDER BY d.dept_name;
```

</details>

---

<details>
<summary class="header">✅ Customer & Orders</summary>

---

## 📌 **11. Customers who never placed an order**

### ✅ Correct Approaches

```sql
SELECT c.*
FROM Customers c
LEFT JOIN Orders o ON o.cust_id = c.cust_id
WHERE o.order_id IS NULL;
```

---

## 📌 **12. Top 5 customers by total order amount**

### ✅ Correct Approaches

#### **Option 1:**

```sql
-- it directly aggregates all order items per customer in one step.
WITH order_totals AS (
  SELECT o.cust_id, SUM(oi.qty * oi.price) AS total_amount
  FROM Orders o
  JOIN OrderItems oi ON oi.order_id = o.order_id
  GROUP BY o.cust_id
)
SELECT TOP (5) c.cust_id, c.cust_name, ot.total_amount
FROM order_totals ot
JOIN Customers c ON c.cust_id = ot.cust_id
ORDER BY ot.total_amount DESC;
```

#### **Option 2:**

```sql
SELECT TOP 5
    c.cust_id,
    c.cust_name,
    SUM(oi.qty * oi.price) AS total_amount
FROM Customers c
JOIN Orders o
    ON c.cust_id = o.cust_id
JOIN OrderItems oi
    ON o.order_id = oi.order_id
GROUP BY c.cust_id, c.cust_name
ORDER BY total_amount DESC;
```

---

## 📌 **13. Customers who ordered in both 2022 and 2023**

### ❌ Wrong Approaches

```sql
SELECT c.*
FROM Customers c
Join Orders o
    On o.cust_id = c.cust_id
WHERE YEAR(o.order_date) IN (2022,2023)
```

> This returns all customers who placed orders in either 2022 or 2023, not necessarily both

---

### ✅ Correct Approaches

#### **Option 1:**

```sql
SELECT
    c.cust_id,
    c.cust_name
FROM Customers c
JOIN Orders o
    ON c.cust_id = o.cust_id
WHERE YEAR(o.order_date) IN (2022, 2023)
GROUP BY c.cust_id, c.cust_name
HAVING COUNT(DISTINCT YEAR(o.order_date)) = 2;
```

#### **Option 2:**

```sql
-- ✅
SELECT c.cust_id, c.cust_name, c.email
FROM Customers c
WHERE c.cust_id IN (
    SELECT cust_id
    FROM Orders
    WHERE YEAR(order_date) = 2022

    INTERSECT

    SELECT cust_id
    FROM Orders
    WHERE YEAR(order_date) = 2023
);
```

---

## 📌 **14. Total number of orders per month**

### ❌ Wrong Approaches

```sql
SELECT
    MONTH(o.order_date) AS order_month,
    COUNT(*) AS total_orders
FROM Orders
GROUP BY order_month
ORDER BY order_month;
```

> ⚡ Tip: If you only group by MONTH(order_date), you’ll merge all Januaries together across years.

---

### ✅ Correct Approaches

```sql
SELECT
    YEAR(o.order_date) AS order_year,
    MONTH(o.order_date) AS order_month,
    COUNT(*) AS total_orders
FROM Orders
GROUP BY order_year, order_month
ORDER BY order_year, order_month;
```

---

## 📌 **15. Customers with the highest number of orders (include ties)**

### ✅ Correct Approaches

#### **Option 1:**

```sql
WITH OrderCounts AS (
    SELECT
        cust_id,
        COUNT(*) AS order_count
    FROM Orders
    GROUP BY cust_id
)
SELECT c.cust_id, c.cust_name, c.email, oc.order_count
FROM OrderCounts oc
JOIN Customers c ON c.cust_id = oc.cust_id
WHERE oc.order_count = (
    SELECT MAX(order_count) FROM OrderCounts
);
```

#### **Option 2:**

```sql
WITH counts AS (
  SELECT cust_id, COUNT(*) AS order_count
  FROM Orders
  GROUP BY cust_id
),
ranked AS (
  SELECT cust_id, order_count,
         RANK() OVER (ORDER BY order_count DESC) AS rnk
  FROM counts
)
SELECT c.cust_id, cu.cust_name, c.order_count
FROM ranked c
JOIN Customers cu ON cu.cust_id = c.cust_id
WHERE rnk = 1;
```

---

## 📌 **16. Average order value per customer**

### ✅ Correct Approaches

```sql
WITH orders_amount AS (
SELECT
    oi.order_id,
    SUM(oi.price * oi.qty) AS order_amount
FROM OrderItems oi
GROUP BY oi.order_id
)
SELECT
    c.cust_id,
    c.cust_name,
    AVG(oa.order_amount) AS avg_order_value
FROM orders_amount oa
Join Orders o
    ON o.order_id = oa.order_id
Join Customers c
    ON c.cust_id = o.cust_id
GROUP BY c.cust_id, c.cust_name
```

---

## 📌 **17. Customers with any order value > 5,000**

### ✅ Correct Approaches

```sql
WITH orders_amount AS (
SELECT
    oi.order_id,
    SUM(oi.price * oi.qty) AS order_amount
FROM OrderItems oi
GROUP BY oi.order_id
)
SELECT DISTINCT
    c.cust_id,
    c.cust_name
FROM orders_amount oa
Join Orders o
    ON o.order_id = oa.order_id
Join Customers c
    ON c.cust_id = o.cust_id
WHERE oa.order_amount > 5000
ORDER BY c.cust_name;
```

---

## 📌 **18. Customers who placed orders but have no email**

### ✅ Correct Approaches

```sql
SELECT DISTINCT *
FROM Customers c
JOIN Orders o ON o.cust_id = c.cust_id
WHERE c.email IS NULL OR LTRIM(RTRIM(c.email)) = '';
```

---

## 📌 **19. Most recent order date per customer**

### ✅ Correct Approaches

#### **Option 1:**

```sql
WITH RankedOrders AS (
    SELECT
        o.order_id,
        o.cust_id,
        o.order_date,
        ROW_NUMBER() OVER (PARTITION BY o.cust_id ORDER BY o.order_date DESC) AS rn
    FROM Orders o
)
SELECT
    ro.cust_id,
    ro.order_id,
    ro.order_date
FROM RankedOrders ro
WHERE ro.rn = 1;
```

#### **Option 2:**

```sql
SELECT o.cust_id, c.cust_name, MAX(o.order_date) AS last_order_date
FROM Orders o
JOIN Customers c ON c.cust_id = o.cust_id
GROUP BY o.cust_id, c.cust_name
ORDER BY last_order_date DESC;
```

</details>

---

<details>
<summary class="header">⌛ Products & Inventory</summary>

---

## 📌 **20. Total sales by product category**

```sql
-- ✅
SELECT p.category, SUM(oi.qty * oi.price) AS total_sales
FROM OrderItems oi
JOIN Products p ON p.product_id = oi.product_id
GROUP BY p.category
ORDER BY total_sales DESC;
```

---

## 📌 **21. Products never ordered**

```sql
-- ❌ Wrong
-- Why? Because you're joining on product_id, and if there's no match, oi.product_id will be NULL. oi.order_id might still exist in other contexts, so it's not the safest null check here.

SELECT
    P.*
FROM Products p
LEFT JOIN OrderItems oi
    on oi.product_id = p.product_id
WHERE oi.order_id IS NULL
```

```sql
-- ✅
SELECT p.*
FROM Products p
LEFT JOIN OrderItems oi ON oi.product_id = p.product_id
WHERE oi.product_id IS NULL;
```

---

## 📌 **22. Top 3 best-selling products by quantity**

```sql
-- ✅
WITH counted AS (
    SELECT
        oi.product_id,
        SUM(oi.quantity) AS total_quantity
    FROM OrderItems oi
    GROUP BY oi.product_id
),
ranked AS (
    SELECT
        product_id,
        ROW_NUMBER() OVER (ORDER BY total_quantity DESC) AS rnk
    FROM counted
)
SELECT p.*
FROM ranked r
JOIN Products p ON p.product_id = r.product_id
WHERE r.rnk <= 3;
```

```sql
-- ✅
SELECT TOP (3) p.product_id, p.product_name,
       SUM(oi.qty) AS total_qty
FROM OrderItems oi
JOIN Products p ON p.product_id = oi.product_id
GROUP BY p.product_id, p.product_name
ORDER BY total_qty DESC, p.product_name;
```

---

## 📌 **23. Products priced above overall average price**

```sql
-- ✅
SELECT *
FROM Products
WHERE price > (
    SELECT AVG(price)
    FROM Products
);
```

---

## 📌 **24. Products out of stock**

```sql
-- ✅
SELECT *
FROM Products
WHERE stock_qty = 0;
```

---

## 📌 **25. Total revenue by product**

```sql
-- ✅
SELECT p.product_id, p.product_name,
       SUM(oi.qty * oi.price) AS revenue
FROM OrderItems oi
JOIN Products p ON p.product_id = oi.product_id
GROUP BY p.product_id, p.product_name
ORDER BY revenue DESC, p.product_name;
```

---

## 📌 **26. Products ordered by more than 10 different customers**

```sql
-- ✅
-- COUNT(DISTINCT o.cust_id): Ensures you're counting unique customers per product.
WITH products_count_per_customer AS (
    SELECT
        oi.product_id,
        COUNT(DISTINCT o.cust_id) AS customer_count
    FROM OrderItems oi
    JOIN Orders o ON o.order_id = oi.order_id
    GROUP BY oi.product_id
)
SELECT
    p.product_id,
    p.product_name,
    customer_count
FROM products_count_per_customer pcpc
JOIN Products p ON p.product_id = pcpc.product_id
WHERE pcpc.customer_count > 10;
```

```sql
-- ✅
SELECT p.product_id, p.product_name,
       COUNT(DISTINCT o.cust_id) AS unique_customers
FROM OrderItems oi
JOIN Orders o   ON o.order_id = oi.order_id
JOIN Products p ON p.product_id = oi.product_id
GROUP BY p.product_id, p.product_name
HAVING COUNT(DISTINCT o.cust_id) > 10
ORDER BY unique_customers DESC;
```

---

## ❌ **27. Products ordered in last 30 days but not in previous 60**

```sql
DECLARE @Today date = CAST(GETDATE() AS date);

-- Last 30 days
WITH last30 AS (
  SELECT DISTINCT oi.product_id
  FROM Orders o
  JOIN OrderItems oi ON oi.order_id = o.order_id
  WHERE o.order_date >= DATEADD(DAY, -30, @Today)
),
prev60 AS (
  SELECT DISTINCT oi.product_id
  FROM Orders o
  JOIN OrderItems oi ON oi.order_id = o.order_id
  WHERE o.order_date >= DATEADD(DAY, -90, @Today)
    AND o.order_date < DATEADD(DAY, -30, @Today)
)
SELECT p.product_id, p.product_name
FROM Products p
JOIN last30 l ON l.product_id = p.product_id
LEFT JOIN prev60 p6 ON p6.product_id = p.product_id
WHERE p6.product_id IS NULL;
```

---

## ❌ **28. Most expensive product in each category**

```sql
WITH ranked AS (
  SELECT category, product_id, product_name, unit_price,
         ROW_NUMBER() OVER (PARTITION BY category ORDER BY unit_price DESC, product_name) AS rn
  FROM Products
)
SELECT category, product_id, product_name, unit_price
FROM ranked
WHERE rn = 1
ORDER BY category;
```

---

## ❌ **29. Products with low stock (stock_qty < 10)**

```sql
SELECT *
FROM Products
WHERE stock_qty < 10
ORDER BY stock_qty ASC, product_name;
```

---

## ❌ **30. Total number of unique products ordered by each customer**

```sql
SELECT c.cust_id, c.cust_name,
       COUNT(DISTINCT oi.product_id) AS unique_products_ordered
FROM Customers c
JOIN Orders o      ON o.cust_id = c.cust_id
JOIN OrderItems oi ON oi.order_id = o.order_id
GROUP BY c.cust_id, c.cust_name
ORDER BY unique_products_ordered DESC, c.cust_name;
```

</details>

<style>
    .header{
        font-size: 24px;
        font-weight: bold;

    }
    .header:hover {
        color: #cfc61aff;
        cursor: pointer;
    }
</style>
