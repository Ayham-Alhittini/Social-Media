export interface MessageListItem {
    chatPhoto: string;
    chatName: string;
    content: string;
    senderUsername: string;
    messageSent: Date;
    unreadCount: number;
    groupName: string;
    isGroupMessage: boolean;
    isSystemMessage: boolean;
}