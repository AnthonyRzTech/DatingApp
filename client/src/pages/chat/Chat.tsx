import { theme } from "@/components/theme";
import { usePayload } from "@/utils/payloadProvider";
import { ChatInterface, Message } from "@/utils/socket";
import { Layout } from "@components/Layout";
import ForumIcon from "@mui/icons-material/Forum";
import {
  Avatar,
  Button,
  Drawer,
  Fab,
  List,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
  Paper,
  Skeleton,
  TextField,
  Typography,
} from "@mui/material";
import { Box, useMediaQuery } from "@mui/system";
import { useEffect, useState } from "react";
import {
  useFetchCurrentUser,
  useFetchUserProfilePic,
} from "../browse/usersActions";
import { User } from "@/app/interfaces";
import LoadingCup from "@/components/LoadingCup/LoadingCup";
import { useFetchUserById } from "../insights/insightsHooks";
import { Send } from "@mui/icons-material";

const ChatDrawerItem = ({
  chat,
  chatUser,
  activeChat,
  setActiveChat,
}: {
  chat: ChatInterface;
  chatUser: number;
  activeChat: number | null;
  setActiveChat: (chatId: number) => void;
}): React.JSX.Element => {
  const {
    data: user,
    isLoading: userLoading,
    isSuccess: userIsSuccess,
  } = useFetchUserById(chatUser);
  const {
    data: picture,
    isLoading: pictureLoading,
    isSuccess: pictureIsSuccess,
  } = useFetchUserProfilePic(chatUser);

  if (userLoading || pictureLoading || !userIsSuccess || !pictureIsSuccess) {
    return <Skeleton variant="rectangular" width="100%" height="64px" />;
  }

  return (
    <ListItemButton
      onClick={() => setActiveChat(chat.id)}
      selected={activeChat === chat.id}
    >
      <ListItemAvatar>
        <Avatar alt={user.username} src={picture.url} />
      </ListItemAvatar>
      <ListItemText
        primary={
          user.username.length > 10
            ? `${user?.username.slice(0, 10)}...`
            : user?.username
        }
      />
    </ListItemButton>
  );
};

export const ChatDrawer = ({
  isMobile,
  chats,
  currentUser,
  activeChat,
  setActiveChat,
}: {
  isMobile: boolean;
  chats: ChatInterface[];
  currentUser: User;
  activeChat: number | null;
  setActiveChat: (chatId: number) => void;
}) => {
  const [drawerOpen, setDrawerOpen] = useState(false);

  let drawerList = (
    <List>
      {chats.map((chat: ChatInterface) => (
        <ChatDrawerItem
          key={chat.id}
          chat={chat}
          chatUser={
            chat.user1Id === currentUser.id ? chat.user2Id : chat.user1Id
          }
          activeChat={activeChat}
          setActiveChat={setActiveChat}
        />
      ))}
    </List>
  );

  let drawer;
  drawer = (
    <>
      {isMobile && (
        <Fab
          color="primary"
          sx={{
            position: "fixed",
            top: "12px",
            left: "12px",
            zIndex: 1,
          }}
          onClick={() =>
            drawerOpen ? setDrawerOpen(false) : setDrawerOpen(true)
          }
        >
          <ForumIcon />
        </Fab>
      )}
      <Drawer
        variant={isMobile ? "temporary" : "permanent"}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        ModalProps={{ keepMounted: true }}
        sx={{
          zIndex: 1,
        }}
      >
        {drawerList}
      </Drawer>
    </>
  );

  return drawer;
};

const ChatInput = ({
  chatId,
  currentUser,
  chatNewMessage,
}: {
  chatId: number;
  currentUser: User;
  chatNewMessage: any;
}) => {
  const [newMessage, setNewMessage] = useState("");

  const handleSendMessage = () => {
    if (!newMessage) return;

    const msgToSend: Message = {
      id: -1, // Will be updated by the server
      chatId: chatId,
      content: newMessage,
      fromUserId: currentUser.id, // Will be checked by the server
      createdAt: new Date(), // Will be updated by the server
    };
    chatNewMessage(msgToSend);
    setNewMessage("");
  };

  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        padding: "8px",
        // height: "calc(15vh)",
        backgroundColor: theme.palette.background.paper,
        borderRadius: "16px",
      }}
    >
      <TextField
        sx={{ flexGrow: 2 }}
        fullWidth
        name="newMessage"
        id="newMessage"
        variant="outlined"
        margin="normal"
        multiline
        slotProps={{ htmlInput: { maxLength: 1000 } }}
        placeholder="Type your message here"
        value={newMessage}
        onChange={(e) => setNewMessage(e.target.value)}
        helperText={`${newMessage.length}/1000`}
      />
      <Button
        variant="contained"
        onClick={handleSendMessage}
        id="sendButton"
        size="small"
        sx={{ marginLeft: "8px", height: "100%" }}
      >
        <Send />
      </Button>
    </Box>
  );
};

export const Chat = ({
  chatId,
  chatNewMessage,
  messagesMarkAsRead,
  currentUser,
}: {
  chatId: number;
  chatNewMessage: (message: Message) => void;
  messagesMarkAsRead: (messageIds: number[]) => void;
  currentUser: User;
}): React.JSX.Element => {
  const { chats } = usePayload();
  const chat = chats?.find((chat) => chat.id === chatId);
  const messages = chat?.messages;

  // Mark messages as read when chat is opened
  useEffect(() => {
    if (chat && messages) {
      // console.log("Mark as read running, triggered by:", new Error().stack);
      // Make a list of unread messages
      const unreadMessages = messages.filter(
        (msg) => !msg.isRead && msg.fromUserId !== currentUser.id
      );
      if (unreadMessages.length > 0) {
        console.log("Unread messages:", unreadMessages);
        messagesMarkAsRead(unreadMessages.map((msg) => msg.id));
      }
    }
  }, [chat, messages, currentUser.id, messagesMarkAsRead]);

  if (!chat || !messages) {
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          width: "100%",
          height: "80vh",
        }}
      >
        <Typography variant="h4">
          ⬅️ Select a chat (or chat not found)
        </Typography>
      </Box>
    );
  }

  const ChatArea = () => {
    return (
      <Box
        sx={{
          // height: "80%", // Changed from calc(100vh - 200px)
          height: "calc(80vh)",
          overflowY: "scroll",
          padding: "16px",
          marginBottom: "16px",
          // backgroundColor: alpha(theme.palette.background.paper, 0.1),
        }}
        ref={(el: HTMLDivElement | null) => {
          if (el) {
            el.scrollTop = el.scrollHeight;
          }
        }}
      >
        {messages.map((message) => (
          <Box
            key={message.id}
            sx={{
              display: "flex",
              justifyContent:
                currentUser.id === message.fromUserId
                  ? "flex-end"
                  : "flex-start",
              marginBottom: "16px",
            }}
          >
            <Paper
              sx={{
                padding: "8px",
                backgroundColor:
                  currentUser.id === message.fromUserId
                    ? theme.palette.primary.main
                    : theme.palette.secondary.main,
              }}
            >
              <Typography
                variant="body2"
                sx={{
                  color:
                    currentUser.id === message.fromUserId ? "black" : "white",
                }}
              >
                {message.content}
              </Typography>
              <Typography
                variant="caption"
                sx={{
                  color: theme.palette.text.secondary,
                }}
              >
                {new Date(message.createdAt).toLocaleString()}
              </Typography>
            </Paper>
          </Box>
        ))}
      </Box>
    );
  };
  return (
    <Box
      sx={{
        width: "100%",
        height: "100%",
      }}
    >
      <ChatArea />
      <ChatInput
        chatId={chatId}
        currentUser={currentUser}
        chatNewMessage={chatNewMessage}
      />
    </Box>
  );
};

export const ChatPage = () => {
  const { chats, chatNewMessage, messagesMarkAsRead } = usePayload();

  const [activeChat, setActiveChat] = useState<number | null>(() => {
    const saved = localStorage.getItem("activeChat");
    return saved ? parseInt(saved) : null;
  });
  useEffect(() => {
    localStorage.setItem("activeChat", activeChat?.toString() || "");
  }, [activeChat]);

  const {
    data: userData,
    isLoading: userDataLoading,
    isSuccess: userDataIsSuccess,
  } = useFetchCurrentUser();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));

  let content;
  if (userDataLoading) {
    content = <LoadingCup />;
  }

  // TODO: Create event listener for new chat messages
  if (userDataIsSuccess && userData && chats) {
    if (chats.length === 0) {
      content = (
        <>
          <Typography
            variant="h4"
            textAlign={"center"}
            padding={"16px"}
            color="text.secondary"
          >
            To chat with someone, you need to match with them first 🍵
          </Typography>
        </>
      );
    } else {
      content = (
        <>
          <Box
            sx={{
              display: "flex",
              flexDirection: "row",
              width: "100%",
              gap: "8px",
            }}
            ref={(el: HTMLDivElement | null) => {
              if (el) {
                el.scrollTop = el.scrollHeight;
              }
            }}
          >
            <Box minWidth={isMobile ? 0 : 150}>
              <ChatDrawer
                isMobile={isMobile}
                chats={chats}
                currentUser={userData}
                activeChat={activeChat}
                setActiveChat={setActiveChat}
              />
            </Box>
            {activeChat ? (
              <Chat
                chatId={activeChat}
                currentUser={userData}
                chatNewMessage={chatNewMessage}
                messagesMarkAsRead={messagesMarkAsRead}
              />
            ) : (
              <Box
                sx={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  width: "100%",
                  height: "80vh",
                }}
              >
                <Typography variant="h4">Select a chat</Typography>
              </Box>
            )}
          </Box>
        </>
      );
    }
  }

  return (
    <Layout>
      <Box
        sx={{
          display: "flex",
          flexDirection: { xs: "column", md: "row" }, // Column on mobile, row on desktop
          justifyContent: "center",
          alignItems: {
            xs: "center",
            md: "flex-end",
          },
          height: "100%",
          gap: {
            xs: "24px", // Vertical gap for mobile
            md: "10vw", // Horizontal gap for desktop
          },
        }}
      >
        {content}
      </Box>
    </Layout>
  );
};
