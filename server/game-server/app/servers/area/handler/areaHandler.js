var areaRemote = require('../remote/areaRemote');

module.exports = function(app) {
	return new Handler(app);
};

var Handler = function(app) {
	this.app = app;
};

var handler = Handler.prototype;

handler.enterScene = function(msg, session, next) {
	var channelService = this.app.get('channelService');

	channel = channelService.getChannel(session.get('rid'), true);

	channel.leave(session.get('uid'), session.frontendId);

	session.set('rid', msg.area);
	session.pushAll();

	channel = channelService.getChannel(msg.area, true);
	
	channel.add(session.uid, session.frontendId);
	return next();
};

handler.enterArea = function(msg, session, next) {
	var channelService = this.app.get('channelService');

	var channel = channelService.getChannel(session.get('rid'), false);

	var param = {
		route: 'onLeave',
		user: session.get('playername')
	};
	channel.pushMessage(param);

	channel.leave(session.get('uid'), session.frontendId);

	return next();
};

handler.move = function(msg, session, next) {
	var rid = session.get('rid');
	var serverId = session.frontendId;
	var username = session.uid.split('*')[0];
	var channelService = this.app.get('channelService');

	channel = channelService.getChannel(msg.area, false);
	console.log("MOVE", session.get('uid'),session.get('rid'), serverId, username,msg.area);

	var param = {
		posx: msg.posx,
		posy: msg.posy,
		dir: msg.dir,
		from: username,
		target: msg.target
	};

	//the target is all users
	if(msg.target != '*') {
		console.log(param);
		channel.pushMessage('onMove', param);
	}
	//the target is specific user
	else {
		var tuid = msg.area;
		var tsid = channel.getMembers(tuid)['sid'];
		console.log(tsid);
		channelService.pushMessageByUids('onMove', param, [{
			uid: tuid,
			sid: tsid
		}]);
	}
	next(null, {
		route: msg.route
	});
};