# Epic 12 - Notifications

Implement user notifications.

## General requirements
Please use the following Angular Front-end: [prolanguage-ui-app](prolanguage-ui-app) 

System should support the following features: 
* Multiple notification methods.
* Order status and learning activity notifications (e.g., homework assignments, new lessons).

## Additional requirements
#### Notifications source
	Managers and users (students/order owners) get notifications on every order status change, or when learning activities are updated.
#### Notification methods
	There are three notification methods: SMS (should be faked for now), push notification (should be faked for now), and email (should be implemented)
#### Preferred notification methods
	Users can select preferable notification methods. 	


## Task Description

### E12 US1 - User story 1

Get notification methods endpoint.
```xml
Url: /users/notifications
Type: GET
Response example:
[
  "sms",
  "push",
  "email"
]
```

### E12 US2 - User story 2
Get user selected notification methods endpoint.
```xml
Url: /users/my/notifications
Type: GET
Response example:
[
  "push",
  "email"
]
```

### E12 US3 - User story 3
Update user selected notification methods endpoint.
```xml
Url: /users/notifications
Type: PUT
Request Example:
{
  "notifications": [
    "sms",
    "email"
  ]
}
```


### E12 US4 - User story 4

Implement email notification infrastructure.

## Non-functional requirement (Optional)

**E12 NFR1**
Use Azure Service Bus as part of email infrastructure.
