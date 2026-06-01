namespace Models;

public record NotificationRequest(string NotificationType, string Sender, string Receiver, string Message);