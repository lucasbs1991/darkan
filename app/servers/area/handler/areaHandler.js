var areaRemote = require('../remote/areaRemote');

module.exports = function(app) {
	return new Handler(app);
};

var Handler = function(app) {
	this.app = app;
};

var handler = Handler.prototype;

handler.leaveScene = function(msg, session, next) {
	var channelService = this.app.get('channelService');

	channel = channelService.getChannel(session.get('rid'), false);

	var param = {
		route: 'onLeave',
		user: session.get('playername')
	};
	channel.leave(session.get('uid'), session.frontendId);
	channel.pushMessage(param);

	session.set('rid', msg.area);
	session.pushAll();

	channel = channelService.getChannel(msg.area, true);

	var member = channel.getMember(session.uid);
	if(!member){
		var param = {
			route: 'onAdd',
			user: session.get('playername')
			// TODO - send other player stats here
		};
		channel.pushMessage(param);
		channel.add(session.uid, session.frontendId);
	}

	//console.log('leave channelService');
	// console.log(channelService);
	//console.log(channel.getMembers());

	var users = channel.getMembers();
	for(var i = 0; i < users.length; i++) {
		users[i] = users[i].split('*')[0];
	}
	channelService.destroyChannel(msg.area);

	// console.log('users on new channel');
	// console.log(users);

	next(null, {
		users:users
	});
};

handler.enterScene = function(msg, session, next) {
	var self = this;

	var channelService = this.app.get('channelService');

	//channelService.pushMessageByUids('onMonsters', {route: 'onMonsters', monsters: 'teste'}, {uid: session.uid, sid: session.frontendId}, errHandler);

	channel = channelService.getChannel(msg.area, true);

	var member = channel.getMember(session.uid);
	if(!member){
		// send message to all players on this channel
		var param = {
			route: 'onAdd',
			user: session.get('playername')
			// TODO - send other player stats here
		};
		channel.pushMessage(param);
		channel.add(session.uid, session.frontendId);
	}

	//console.log('enter channelService');
	//console.log(channelService);

	var monsters = self.app.get('monsters');
	//monsters[0].posx = 999;
	//console.log(monsters[0]);

	var users = channel.getMembers();
	var data = [];
	for(var i = 0; i < users.length; i++) {
		var user = channel.getMember(users[i]);
		user.uid = user.uid.split('*')[0];
		data.push(user);
	}
	//console.log(data);
	next(null, {
		users:data,
		monsters:monsters
	});
};

handler.move = function(msg, session, next) {
	var rid = session.get('rid');
	var serverId = session.frontendId;
	var username = session.uid.split('*')[0];
	var channelService = this.app.get('channelService');

	channel = channelService.getChannel(rid, true);
	var member = channel.getMember(session.uid);
	if(!member)
		channel.add(session.uid, session.frontendId);

	var x = 0;
	var y = 0;
	if(msg.dir == "left")
		x = -1;
	else if(msg.dir == "right")
		x = 1;
	else if(msg.dir == "up")
		y = 1;
	else if(msg.dir == "down")
		y = -1;
	member.posx = (parseInt(msg.posx) + x).toString();
	member.posy = (parseInt(msg.posy) + y).toString();

	// console.log(channel);
	//console.log("MOVE", session.get('uid'),session.get('rid'), serverId, username,rid);

	var param = {
		posx: msg.posx,
		posy: msg.posy,
		dir: msg.dir,
		from: username,
		target: msg.target
	};

	// console.log('move channelService');
	// console.log(channelService);
	// console.log(channel.getMembers());

	//the target is all users
	if(msg.target != '*') {
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
	return next();
	next(null, {
		route: msg.route
	});
};

handler.monsterMove = function(msg, session, next) {
	var rid = session.get('rid');
	var serverId = session.frontendId;
	var username = session.uid.split('*')[0];
	var channelService = this.app.get('channelService');

	channel = channelService.getChannel(rid, true);

	var monsters = this.app.get('monsters');
	var monster = monsters.find(x => x._id == msg.id);
	monster.posx += parseInt(msg.posx);
	monster.posy += parseInt(msg.posy);

	var param = {
		id: msg.id,
		posx: msg.posx,
		posy: msg.posy
	};

	// console.log('move channelService');
	// console.log(channelService);
	// console.log(channel.getMembers());

	//the target is all users
	if(msg.target != '*') {
		channel.pushMessage('onMonsterMove', param);
	}

	return next();
};

function errHandler(err, fails){
	if(!!err){
		logger.error('Push Message error! %j', err.stack);
	}
}