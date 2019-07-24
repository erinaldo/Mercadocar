-- connect handler
function this.TcpClientConnected(this, context)
	this:SendString(context, "==> Login")
end

-- disconnecting handler (connection is still alive)
function this.TcpClientDisconnecting(this, context)
	this:SendString(context, "==> Logout")
end

-- disconnect handler (connection is already broken)
function this.TcpClientDisconnected(this, context)
	return
end
