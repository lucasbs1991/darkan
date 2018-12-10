module.exports = function(app) {
	return new Handler(app);
};

var Handler = function(app) {
		this.app = app;
};

var handler = Handler.prototype;

/**
 * New client entry chat server.
 *
 * @param  {Object}   msg     request message
 * @param  {Object}   session current session object
 * @param  {Function} next    next stemp callback
 * @return {Void}
 */
handler.enter = function(msg, session, next) {
	var self = this;
	var rid = msg.rid;
	// var uid = msg.username + '*' + rid;
	var uid = msg.username + '*global';
	var sessionService = self.app.get('sessionService');

	//duplicate log in
	if( !! sessionService.getByUid(uid)) {
		next(null, {
			code: 500,
			error: true
		});
		return;
	}

	console.log(self.app.get('areaIdMap'));
	console.log(msg.areaServer);
	session.bind(uid);
	session.set('serverId', self.app.get('areaIdMap')[msg.areaServer]); // player.areaId
	session.set('rid', rid);
	session.set('uid', uid);
	session.set('playername', msg.username);
	// session.push('rid', function(err) {
	// 	if(err) {
	// 		console.error('set rid for session service failed! error is : %j', err.stack);
	// 	}
	// });
	session.on('closed', onUserLeave.bind(null, self.app));
	session.pushAll();
	console.log(session);

	//put user into channel
	self.app.rpc.chat.chatRemote.add(session, uid, self.app.get('serverId'), rid, true, function(users){
		next(null, {
			users:users
		});
	});
};

/**
 * User log out handler
 *
 * @param {Object} app current application
 * @param {Object} session current session object
 *
 */
var onUserLeave = function(app, session) {
	if(!session || !session.uid) {
		return;
	}

	app.rpc.area.areaRemote.playerLeave(session, session.uid, app.get('serverId'), session.get('rid'), null);

	app.rpc.chat.chatRemote.kick(session, session.uid, app.get('serverId'), session.get('rid'), null);
};