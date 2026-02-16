namespace CorporateBrain.Domain;

public enum DocumentStatus
{
    Uploaded = 1, // File received, nothing happened yet
    Processing = 2, // AI is reading it (Vectorizing)
    Ready = 3, // Ready for chat
    Failed = 4 // Something broke
}
