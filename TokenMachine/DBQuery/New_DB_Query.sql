

--create database Product_Store;

use database Product_Store;

select * from mst_product;

--update mst_product p set p.stock = 5 , soldout = 0  where p.product_id = 102

--delete from mst_product where product_id = 106


CREATE TABLE mst_product(
    product_id bigint NOT NULL AUTO_INCREMENT,
    product_name varchar(255),
    price decimal,
    img_path varchar(255),
    stock bigint,
    soldout bit,
    PRIMARY KEY (product_id)
);

SELECT product_id, product_name ,price, sum(stock) as stock, img_path  from (
	Select product_id, product_name ,price, img_path,
    case when soldout > 0 then 0 else stock end as stock from mst_product p
) as M group by M.product_id;


SELECT product_id, product_name , sum(stock) as 'Available InStock',OutOfStock    from (
	Select product_id, product_name ,price, 
    case when soldout > 0 then 0 else stock end as stock,
    case when soldout > 0 then 'Yes' else 'No' end as OutOfStock
    from mst_product p
) as M group by M.product_id;


--insert into mst_product values(101,'Little Hearts',10,'/Images/LittleHearts.jpg',20,0);
--insert into mst_product values(102,'Lays Green',10,'/Images/Lays-Green.png',20,0);
--insert into mst_product values(103,'Bounce',10,'/Images/Bounce.jpg',20,0);
--insert into mst_product values(104,'Dark Fantasy',10,'/Images/DarkFantasy.jpg',20,0);
--insert into mst_product values(105,'Lays Red',10,'/Images/Lays-Red.jpg',20,0);
--insert into mst_product values(106,'HideNSeek',10,'/Images/Lays-Red.jpg',20,0);


--update mst_product set img_path='/Images/LittleHearts.png' where product_id=101;
--update mst_product set img_path='/Images/Lays-Green.png' where product_id=102;
--update mst_product set img_path='/Images/Bounce.png' where product_id=103;
--update mst_product set img_path='/Images/DarkFantasy.png' where product_id=104;
--update mst_product set img_path='/Images/Lays-Red.png' where product_id=105;

CREATE TABLE paytm_upi (
  id int(11) NOT NULL AUTO_INCREMENT,
  order_date datetime DEFAULT NULL,
  order_id varchar(50) DEFAULT NULL,
  order_amount decimal(6,2) DEFAULT NULL,
  qr_status_code VARCHAR(10) DEFAULT NULL,
  qr_status_msg varchar(50) DEFAULT NULL,
  transaction_id varchar(50) DEFAULT NULL,
  transaction_date datetime DEFAULT NULL,
  transaction_code VARCHAR(10) DEFAULT NULL,
  transaction_msg varchar(50) DEFAULT NULL,
  sales_code varchar(20) DEFAULT NULL,
  is_refunded bit(1) NOT NULL DEFAULT b'0',
  refund_request_date datetime DEFAULT NULL,
  refund_request_id varchar(50) DEFAULT NULL,
  refund_request_amount decimal(6,2) DEFAULT NULL,
  refund_id varchar(50) DEFAULT NULL,
  refund_code VARCHAR(10) DEFAULT NULL,
  refund_msg varchar(50) DEFAULT NULL,
  PRIMARY KEY (id)
) 


--select current_timestamp;

--select order_id as 'Order ID',product_lineitems as 'Order Details',total_amount as 'Order Amount',
-- total_quantity as 'Order Quantity',order_datetime as 'Order Date',payment_method as 'Payment Method', 
-- transaction_id as 'Transaction ID',machine_id as 'Machine ID'   from sales_order;

--drop table sales_order;


CREATE TABLE sales_order(
    order_id bigint NOT NULL AUTO_INCREMENT,
    product_lineitems varchar(2000) DEFAULT NULL,
    total_amount decimal(6,2) DEFAULT NULL,
    total_quantity int DEFAULT NULL,
    order_datetime datetime DEFAULT NULL,
    payment_method varchar(50) DEFAULT NULL,
    transaction_id varchar(50) DEFAULT NULL,
    machine_id varchar(50) DEFAULT NULL,
    PRIMARY KEY (order_id)
);




















