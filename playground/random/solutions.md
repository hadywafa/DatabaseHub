# Solutions

## **Employee & Salary Related**

### 1) Second highest salary

```sql
SELECT emp_id, emp_name, salary
FROM (
  SELECT e.*, DENSE_RANK() OVER (ORDER BY salary DESC) AS rk
  FROM Employees e
) x
WHERE rk = 2;
```

### 2) Top 3 highest-paid employees

```sql
SELECT emp_name, salary
FROM (
  SELECT emp_name, salary,
         DENSE_RANK() OVER (ORDER BY salary DESC) AS rnk
  FROM Employees
) x
WHERE rnk <= 3
ORDER BY salary DESC, emp_name;
```

### 3) Employees earning above company average

```sql
SELECT e.*
FROM Employees e
CROSS JOIN (SELECT AVG(salary) AS avg_salary FROM Employees) a
WHERE e.salary > a.avg_salary;
```

### 4) Number of employees in each department

```sql
SELECT d.dept_name, COUNT(*) AS emp_count
FROM Departments d
LEFT JOIN Employees e ON e.dept_id = d.dept_id
GROUP BY d.dept_name
ORDER BY d.dept_name;
```

### 5) Departments with avg salary > 10,000

```sql
SELECT d.dept_name, AVG(e.salary) AS avg_salary
FROM Employees e
JOIN Departments d ON d.dept_id = e.dept_id
GROUP BY d.dept_name
HAVING AVG(e.salary) > 10000;
```

### 6) Employees without a department

```sql
SELECT *
FROM Employees
WHERE dept_id IS NULL;
```

### 7) Employees who joined in 2022

```sql
SELECT *
FROM Employees
WHERE YEAR(hire_date) = 2022;
```

### 8) Employees sharing the same salary with someone else

```sql
SELECT e.*
FROM Employees e
JOIN (
  SELECT salary
  FROM Employees
  GROUP BY salary
  HAVING COUNT(*) > 1
) s ON s.salary = e.salary
ORDER BY e.salary DESC, e.emp_name;
```

### 9) Highest-paid employee in each department

```sql
WITH ranked AS (
  SELECT d.dept_name, e.emp_name, e.salary,
         ROW_NUMBER() OVER (PARTITION BY d.dept_name ORDER BY e.salary DESC, e.emp_name) AS rn
  FROM Employees e
  JOIN Departments d ON d.dept_id = e.dept_id
)
SELECT dept_name, emp_name, salary
FROM ranked
WHERE rn = 1
ORDER BY dept_name;
```

### 10) Total salary expense per department

```sql
SELECT d.dept_name, SUM(e.salary) AS total_salary
FROM Employees e
JOIN Departments d ON d.dept_id = e.dept_id
GROUP BY d.dept_name
ORDER BY d.dept_name;
```

---

## **Customer & Orders**

### 11) Customers who never placed an order

```sql
SELECT c.*
FROM Customers c
LEFT JOIN Orders o ON o.cust_id = c.cust_id
WHERE o.order_id IS NULL;
```

### 12) Top 5 customers by total order amount

```sql
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

### 13) Customers who ordered in both 2022 and 2023

```sql
SELECT cust_id
FROM Orders
WHERE YEAR(order_date) IN (2022, 2023)
GROUP BY cust_id
HAVING COUNT(DISTINCT YEAR(order_date)) = 2;
```

### 14) Total number of orders per month

```sql
SELECT DATEFROMPARTS(YEAR(order_date), MONTH(order_date), 1) AS month_start,
       COUNT(*) AS order_count
FROM Orders
GROUP BY DATEFROMPARTS(YEAR(order_date), MONTH(order_date), 1)
ORDER BY month_start;
```

### 15) Customers with the highest number of orders (include ties)

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

### 16) Average order value per customer

```sql
WITH order_totals AS (
  SELECT o.order_id, o.cust_id, SUM(oi.qty * oi.price) AS order_total
  FROM Orders o
  JOIN OrderItems oi ON oi.order_id = o.order_id
  GROUP BY o.order_id, o.cust_id
)
SELECT c.cust_id, c.cust_name,
       AVG(ot.order_total) AS avg_order_value
FROM order_totals ot
JOIN Customers c ON c.cust_id = ot.cust_id
GROUP BY c.cust_id, c.cust_name
ORDER BY avg_order_value DESC;
```

### 17) Customers with any order value > 5,000

```sql
WITH order_totals AS (
  SELECT o.order_id, o.cust_id, SUM(oi.qty * oi.price) AS order_total
  FROM Orders o
  JOIN OrderItems oi ON oi.order_id = o.order_id
  GROUP BY o.order_id, o.cust_id
)
SELECT DISTINCT c.cust_id, c.cust_name
FROM order_totals ot
JOIN Customers c ON c.cust_id = ot.cust_id
WHERE ot.order_total > 5000
ORDER BY c.cust_name;
```

### 18) Customers who placed orders but have no email

```sql
SELECT DISTINCT c.*
FROM Customers c
JOIN Orders o ON o.cust_id = c.cust_id
WHERE c.email IS NULL OR LTRIM(RTRIM(c.email)) = '';
```

### 19) Most recent order date per customer

```sql
SELECT o.cust_id, c.cust_name, MAX(o.order_date) AS last_order_date
FROM Orders o
JOIN Customers c ON c.cust_id = o.cust_id
GROUP BY o.cust_id, c.cust_name
ORDER BY last_order_date DESC;
```

### 20) Total sales by product category

```sql
SELECT p.category, SUM(oi.qty * oi.price) AS total_sales
FROM OrderItems oi
JOIN Products p ON p.product_id = oi.product_id
GROUP BY p.category
ORDER BY total_sales DESC;
```

---

## **Products & Inventory**

### 21) Products never ordered

```sql
SELECT p.*
FROM Products p
LEFT JOIN OrderItems oi ON oi.product_id = p.product_id
WHERE oi.order_id IS NULL;
```

### 22) Top 3 best-selling products by quantity

```sql
SELECT TOP (3) p.product_id, p.product_name,
       SUM(oi.qty) AS total_qty
FROM OrderItems oi
JOIN Products p ON p.product_id = oi.product_id
GROUP BY p.product_id, p.product_name
ORDER BY total_qty DESC, p.product_name;
```

### 23) Products priced above overall average price

```sql
SELECT p.*
FROM Products p
CROSS JOIN (SELECT AVG(unit_price) AS avg_price FROM Products) a
WHERE p.unit_price > a.avg_price
ORDER BY p.unit_price DESC;
```

### 24) Products out of stock

```sql
SELECT *
FROM Products
WHERE stock_qty = 0;
```

### 25) Total revenue by product

```sql
SELECT p.product_id, p.product_name,
       SUM(oi.qty * oi.price) AS revenue
FROM OrderItems oi
JOIN Products p ON p.product_id = oi.product_id
GROUP BY p.product_id, p.product_name
ORDER BY revenue DESC, p.product_name;
```

### 26) Products ordered by more than 10 different customers

```sql
SELECT p.product_id, p.product_name,
       COUNT(DISTINCT o.cust_id) AS unique_customers
FROM OrderItems oi
JOIN Orders o   ON o.order_id = oi.order_id
JOIN Products p ON p.product_id = oi.product_id
GROUP BY p.product_id, p.product_name
HAVING COUNT(DISTINCT o.cust_id) > 10
ORDER BY unique_customers DESC;
```

### 27) Products ordered in last 30 days but not in previous 60

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

### 28) Most expensive product in each category

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

### 29) Products with low stock (stock_qty < 10)

```sql
SELECT *
FROM Products
WHERE stock_qty < 10
ORDER BY stock_qty ASC, product_name;
```

### 30) Total number of unique products ordered by each customer

```sql
SELECT c.cust_id, c.cust_name,
       COUNT(DISTINCT oi.product_id) AS unique_products_ordered
FROM Customers c
JOIN Orders o      ON o.cust_id = c.cust_id
JOIN OrderItems oi ON oi.order_id = o.order_id
GROUP BY c.cust_id, c.cust_name
ORDER BY unique_products_ordered DESC, c.cust_name;
```
