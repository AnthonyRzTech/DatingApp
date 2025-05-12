export interface Message {
  id: number;
  content: string;
  chatId: number;
  fromUserId: number;
  createdAt: Date;
  isRead?: boolean;
}

export interface Chat {
  id: number;
  messages: Message[];
  user1Id: number;
  user2Id: number;
  createdAt: Date;
  deletedBy: number[];
}
