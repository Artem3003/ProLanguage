# Epic 10 - AI Chat [Microsoft Azure]
Add AI chat capabilities to support interactive learning assistance.

## General requirements
Please use the following Angular Front-end: [prolanguage-ui-app](prolanguage-ui-app)

System should support the following features:
* Chat with AI assistant.
* Persist conversation history.
* Display real-time or near real-time AI responses.

Technical specifications:
* Use the latest stable version of ASP.NET Core (Web API/MVC template).
* Use N-layer architecture.
* Use built-in service provider.
* Follow SOLID principles.
* Use JSON as request/response format.
* Use MS SQL Server.

---

## Task Description

### E10 US1 - User story 1
Create chat session endpoint.

Request example:
```{xml}
{
	"title": "English Grammar Help"
}
```

Response example:
```{xml}
{
	"id": "a1d4db88-6a20-4cc6-9a84-c1d7a1fa8d91",
	"title": "English Grammar Help",
	"createdAtUtc": "2026-04-22T10:30:00Z"
}
```

### E10 US2 - User story 2
Send message endpoint should accept user input and return AI response.

Request example:
```{xml}
{
	"chatId": "a1d4db88-6a20-4cc6-9a84-c1d7a1fa8d91",
	"message": "Explain present perfect tense with 3 examples.",
	"context": {
		"language": "English",
		"level": "Beginner"
	}
}
```

Response example:
```{xml}
{
	"messageId": "f39d53df-312e-4e40-a7f5-d75ad2d6f74d",
	"chatId": "a1d4db88-6a20-4cc6-9a84-c1d7a1fa8d91",
	"role": "assistant",
	"content": "Present perfect tense is used for actions connected to now...",
	"createdAtUtc": "2026-04-22T10:31:10Z",
	"tokensUsed": 182
}
```

### E10 US3 - User story 3
Get chat messages endpoint.

```{xml}
Url: /chats/{id}/messages
Type: GET
Response: chat messages list sorted by created date ascending
```

### E10 US4 - User story 4
Rename chat session endpoint.

Request example:
```{xml}
{
	"title": "Homework Preparation - Unit 3"
}
```

```{xml}
Url: /chats/{id}
Type: PATCH
```

### E10 US5 - User story 5
Delete chat session endpoint should remove chat and all related messages.

```{xml}
Url: /chats/{id}
Type: DELETE
```

---

## Non-functional requirements

**E10 NFR1**
AI generation should use Azure OpenAI service.

**E10 NFR2**
Implement caching mechanism for chat context assembly (recent messages and system prompt) to reduce latency.

**E10 NFR3**
Apply safety and limits: maximum user message size 4000 characters, per-user rate limit, prompt/content filtering, and timeout handling for AI requests.
