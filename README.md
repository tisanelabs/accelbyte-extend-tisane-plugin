# AccelByte Extend Event Handler - Tisane Integration

This project demonstrates how to integrate [Tisane AI](https://tisane.ai/) with AccelByte Gaming Services for real-time chat content moderation using Extend Event Handler.

## About AccelByte Extend Event Handler

AccelByte Gaming Services provides an Extend Event Handler feature that lets you listen to platform events and execute custom logic. This is a gRPC server that receives events through Kafka Connect.

For complete setup and deployment instructions, see the [official AccelByte documentation](https://docs.accelbyte.io/gaming-services/services/extend/event-handler/getting-started-event-handler/).

## Tisane API Integration

This app listens to personal chat events from AccelByte and automatically analyzes message content using Tisane's API to detect problematic content, sentiment, entities, and more.

### Environment Variables

Configure these environment variables in your AccelByte Extend App:

| Variable | Description | Default Value |
|----------|-------------|---------------|
| `TISANE_API_KEY` | Your Tisane API subscription key from [Tisane Developer Portal](https://tisane.ai/) | *Required - No default* |
| `TISANE_PARSE_URL` | Tisane API endpoint for text analysis | `https://api.tisane.ai/parse` |
| `TISANE_LANGUAGE_CODES` | Language code for content analysis (e.g., `en`, `es`, `fr`) | `en` |
| `TISANE_SETTING_FORMAT` | Content format type. Use `dialogue` for chat messages | `dialogue` |

### Getting Your Tisane API Key

1. Visit [https://tisane.ai/](https://tisane.ai/)
2. Sign up for an account or log in
3. Go to the [Developer Portal](https://dev.tisane.ai/profile)
4. Copy your API subscription key (primary or secondary)

### Configuration in AccelByte Admin Portal

1. Navigate to your Extend Event Handler App
2. Go to **Environment Configuration**
3. Under **Secrets**, add:
   - `TISANE_API_KEY` = `your-tisane-api-key`
4. Under **Variables** (optional), add:
   - `TISANE_PARSE_URL` = `https://api.tisane.ai/parse`
   - `TISANE_LANGUAGE_CODES` = `en`
   - `TISANE_SETTING_FORMAT` = `dialogue`

### How It Works

When a user sends a personal chat message:

1. `PersonalChatSentService` receives the chat event from AccelByte
2. Message content is extracted from the event payload
3. Request is sent to Tisane API with the message
4. Tisane analyzes the content for:
   - Abusive language and problematic content
   - Sentiment analysis
   - Named entities
   - Topics and themes
   - Language detection
5. Analysis results are logged and can be used for moderation decisions

### Local Testing

For local development, create a `.env` file with:

```
AB_BASE_URL=https://demo.accelbyte.io
AB_CLIENT_ID='your-client-id'
AB_CLIENT_SECRET='your-client-secret'
AB_NAMESPACE='your-namespace'
ITEM_ID_TO_GRANT='your-item-id'

# Tisane Configuration
TISANE_API_KEY='your-tisane-api-key'
TISANE_PARSE_URL=https://api.tisane.ai/parse
TISANE_LANGUAGE_CODES=en
TISANE_SETTING_FORMAT=dialogue
```

Run the app:

```shell
docker compose up --build
```

## Additional Resources

- [Tisane API Documentation](https://docs.tisane.ai)
- [AccelByte Extend Event Handler Guide](https://docs.accelbyte.io/gaming-services/services/extend/event-handler/getting-started-event-handler/)
