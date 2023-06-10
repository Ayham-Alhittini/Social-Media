export interface Message {
    id: number
    senderId: string
    senderUsername: string
    senderPhotoUrl: string
    recipenetId: string
    recipenetUsername: string
    recipenetPhotoUrl: string
    content: string
    dateRead?: Date
    messageSent: Date
    unreadCount: number
  }
  