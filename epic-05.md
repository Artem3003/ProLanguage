# Epic 5 - Payment Methods  
Extend the functionality of the **Pro Language** web application by adding the ability to pay for language courses and subscriptions.

## General Requirements  
Use the existing Angular Front-end: [prolanguage-ui-app](prolanguage-ui-app)  
Integrate with the following [Microservice payment](microservice\publish)

The system should support the following features:  
* Add course to cart.  
* Get courses from cart.  
* Get orders.  
* Get payment methods.  
* Perform payment with the selected method.  

---

### Entities  

#### **Order**  
* **Id:** Guid, required, unique  
* **Date:** DateTime, optional  
* **CustomerId:** Guid, required  
* **Status:** Enum, required  

#### **OrderCourse**  
* **OrderId:** Guid, required  
* **CourseId:** Guid, required  
* **Price:** Double, required  
* **Quantity:** Int, required  
* **Discount:** Int, optional  

Course-Order combinations are unique.  

---

## Additional Requirements  

### Payment  
Create a single endpoint for payment, independent from the selected method.  
If the payment is processed successfully, the order must be marked as **Paid**, otherwise **Cancelled**.  

### Payment Methods  
Allowed payment methods: **“Bank”**, **“IBox terminal”**, **“Visa”**.  
Only one method can be applied to an order for payment.  
Each payment method includes a small image, title, and description.  

### Customer  
Since user registration is still under development, use a stub value for **Customer Id**.  

### Bank Payment Validity  
The **invoice validity date** (how long the invoice is valid) must be configurable via application settings.  

### Order Limitations  
Users cannot order more courses than available (if limited enrollment applies).  

### Order Statuses  
* **Open** – courses are in the cart.  
* **Checkout** – payment is started.  
* **Paid** – payment is performed successfully.  
* **Cancelled** – payment failed.  

---

## Task Description  

### E05 US1 - Add and Remove Courses from Cart  
**Add Course to Cart Endpoint**  
```http
Url: /courses/{key}/buy  
Type: POST  
Limitation: if the endpoint is called for a course already in the cart, increment quantity  
Response: Success status code
```

**Delete Course from Cart Endpoint
```http
Url: /orders/cart/{key}  
Type: DELETE  
Response: Success status code
```

### E05 US2 - Get Paid and Cancelled Orders
**Get Orders Endpoint
```http
Url: /orders  
Type: GET  
Response example:
[
  {
    "id": "5d8af81a-c146-4588-93bf-0e5c7e9e4a9e",
    "customerId": "5aa1c97e-e6b3-497c-8e00-270e96aa0b63",
    "date": "2023-11-20T11:03:26.0572863+02:00"
  }
]
```

**Get Order by ID Endpoint
```http
Url: /orders/{id}  
Type: GET  
Response example:
{
  "id": "5d8af81a-c146-4588-93bf-0e5c7e9e4a9e",
  "customerId": "5aa1c97e-e6b3-497c-8e00-270e96aa0b63",
  "date": "2023-11-20T11:03:26.0572863+02:00"
}
```
### E05 US3 - Get Order Details
```http
Url: /orders/{id}/details  
Type: GET  
Response Example:
[
  {
    "courseId": "8ea595c2-765b-4190-b3da-da00540b2202",
    "price": 100,
    "quantity": 1,
    "discount": 0
  },
  {
    "courseId": "923a8bd8-b256-44f4-b972-a0d640d56ef4",
    "price": 50,
    "quantity": 2,
    "discount": 10
  }
]
```

### E05 US4 - Get Cart
```http
Url: /orders/cart  
Type: GET  
Hint: Cart is an order in status Open. If no open order exists, create one automatically when the first course is added.  
Response example:
[
  {
    "courseId": "f6f698ee-41df-4594-b90c-3862e7d2cbee",
    "price": 30,
    "quantity": 1,
    "discount": 0
  },
  {
    "courseId": "75396383-c1fa-4cbd-81c9-c874ae3a3e67",
    "price": 100,
    "quantity": 1,
    "discount": 20
  }
]
```

### E05 US5 - Get Payment Methods
```http
Url: /orders/payment-methods  
Type: GET  
Response Example:
{
  "paymentMethods": [
    {
      "imageUrl": "image link1",
      "title": "Bank",
      "description": "Payment via bank transfer. Invoice provided."
    },
    {
      "imageUrl": "image link2",
      "title": "IBox terminal",
      "description": "Pay easily through a terminal near you."
    },
    {
      "imageUrl": "image link3",
      "title": "Visa",
      "description": "Instant online card payment."
    }
  ]
}
```

### E05 US6 - "Bank" Payment
```http
Url: /orders/payment  
Type: POST  
Request: {"method": "Bank"}  
Flow: The system should return a generated invoice (PDF) containing:
- User ID  
- Order ID  
- Creation date  
- Validity date  
- Total sum
```

### E05 US7 - "IBox Terminal" Payment
```http
Url: /orders/payment  
Type: POST  
Request: {"method": "IBox terminal"}  
Integration: With payment microservice  
Flow: Handle requests for IBox payments.  
Response Example:
{
  "userId": "24967e32-dec1-47b5-8ca6-478afa84c2be",
  "orderId": "7dce8347-4181-4316-9210-302361340975",
  "paymentDate": "2023-11-18T11:03:26.0575052+02:00",
  "sum": 150
}
```

### E05 US8 - "Visa" Payment
```http
Url: /orders/payment  
Type: POST  
Request:
{
  "method": "Visa",
  "model": {
    "holder": "Artem Yurchenko",
    "cardNumber": "123321122344231",
    "monthExpire": 10,
    "yearExpire": 2030,
    "cvv2": 111
  }
}
Integration: With payment microservice  
Flow: Handle Visa payments with full card validation.  
Response: Success status code.
```

### Non-functional Requirements
** E05 NFR1 **

A Payment Microservice must run locally and integrate with the Pro Language API for Visa and IBox payments.
Swagger endpoint should be used to test and validate requests.

** E05 NFR2 **

Implement fault-tolerant payment acceptance.
As the microservice may reject up to 10% of transactions, add response validation and automatic retry on failure.
Ensure compatibility with all microservice responses.
