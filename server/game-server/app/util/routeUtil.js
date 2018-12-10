var exp = module.exports;
var dispatcher = require('./dispatcher');

exp.area = function(session, msg, app, cb) {
	var serverId = session.get('serverId');

	if(!serverId) {
		cb(new Error('can not find server info for type: ' + msg.serverType));
		return;
	}

	cb(null, serverId);
};

exp.chat = function(session, msg, app, cb) {
	var chatServers = app.getServersByType('chat');

	if(!chatServers || chatServers.length === 0) {
		cb(new Error('can not find chat servers.'));
		return;
	}

	var res = dispatcher.dispatch(session.get('rid'), chatServers);

	cb(null, res.id);
};