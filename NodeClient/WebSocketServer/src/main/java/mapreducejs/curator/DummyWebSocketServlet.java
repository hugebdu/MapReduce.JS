package mapreducejs.curator;

import org.eclipse.jetty.websocket.WebSocket;
import org.eclipse.jetty.websocket.WebSocketServlet;

import javax.servlet.http.HttpServletRequest;
import java.io.IOException;
import java.util.Date;
import java.util.Timer;
import java.util.TimerTask;

/**
 * Created by IntelliJ IDEA.
 * User: daniels
 * Date: 7/10/12
 */
public class DummyWebSocketServlet extends WebSocketServlet
{
    @Override
    public WebSocket doWebSocketConnect(HttpServletRequest httpServletRequest, String s)
    {
        return new SomeWebSocket();
    }

    static class SomeWebSocket extends TimerTask implements WebSocket.OnTextMessage
    {
        private Connection connection;

        public SomeWebSocket()
        {
            new Timer().scheduleAtFixedRate(this, 5000, 5000);
        }

        @Override
        public void run()
        {
            if (connection != null)
            {
                try
                {
                    connection.sendMessage("Moscow time is: " + new Date().toString());
                }
                catch (IOException e)
                {
                    e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
                }
            }
        }

        @Override
        public void onMessage(String data)
        {
            System.out.println(data);
        }

        @Override
        public void onOpen(Connection connection)
        {
            this.connection = connection;
            System.out.println("onOpen");
        }

        @Override
        public void onClose(int closeCode, String message)
        {
            System.out.println("onClose");
        }
    }
}
