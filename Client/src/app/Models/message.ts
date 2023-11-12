export interface Message {
  id: number;
  senderUsername: string;
  senderPhotoUrl: string;
  content: string;
  groupName: string;
  messageSent: string;
  isGroupMessage: boolean;
  isSystemMessage: boolean;
}