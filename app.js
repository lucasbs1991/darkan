var pomelo = require('pomelo');
var routeUtil = require('./app/util/routeUtil');
var MongoClient = require('mongodb').MongoClient;
/**
 * Init app for client.
 */
var app = pomelo.createApp();
app.set('name', 'darkan');

var userscollection;
var monsterscollection;

// app configuration
app.configure('production|development', 'connector', function(){
	app.set('connectorConfig',
		{
			connector : pomelo.connectors.hybridconnector,
			heartbeat : 3,
			useDict : true,
			useProtobuf : true
		});
});

// Configure database
app.configure('production|development', 'area|connector|master', async function() {
    var database = await MongoClient.connect("mongodb://darkanadmin:darkan666@tatooine.mongodb.umbler.com:54170/darkan", { useNewUrlParser: true });

    var db = await database.db('darkan');
    this.userscollection = await db.collection('users');
    var dados = await this.userscollection.find({ name: "Lucas" }).toArray();
    //console.log(dados);

    //var teste = bcrypt.compareSync('182512', dados[0].password);
    //console.log(teste);
});

app.configure('production|development', 'area', async function(){
	var database = await MongoClient.connect("mongodb://darkanadmin:darkan666@tatooine.mongodb.umbler.com:54170/darkan", { useNewUrlParser: true });

    var db = await database.db('darkan');
    var collection = await db.collection('monsters');
    var monsters = await collection.find().toArray();
    app.set('monsters', monsters);
    //console.log(monsters);
});

app.configure('production|development', 'gate', function(){
	app.set('connectorConfig',
		{
			connector : pomelo.connectors.hybridconnector,
			useDict: true,
			useProtobuf : true
		});
});

// app configure
app.configure('production|development', function() {
	if (app.serverType !== 'master') {
		var areas = app.get('servers').area;
		var areaIdMap = {};
		for(var id in areas){
			areaIdMap[areas[id].area] = areas[id].id;
		}
		app.set('areaIdMap', areaIdMap);
	}

	// route configures
	app.route('chat', routeUtil.chat);
	app.route('area', routeUtil.area);

	// filter configures
	app.filter(pomelo.timeout());
});

// start app
app.start();

process.on('uncaughtException', function(err) {
	console.error(' Caught exception: ' + err.stack);
});