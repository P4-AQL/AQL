grammar AQL;

program: importStatement* definition? EOF;
importStatement: 'import' identifier;

definition:
	functionDefinition definition?
	| constDefinition definition?
	| networks definition?
	| simulateDefinition;

functionDefinition:
	'function' returnType = type identifier '(' formalParameterList? ')' block;

constDefinition: 'const' type assignStatement;

formalParameterList: type identifier (',' type identifier)*;

networks: queueDefinition | networkDefinition;

queueDefinition:
	'queue' identifier '{' (
		'servers:' numberOfServers = expression ';'
	)? 'service:' service = expression ';' 'capacity:' capacity = expression (
		';' (metrics ';'?)?
	)? '}';

networkDefinition:
	'network' identifier '{' networkExpression? (
		';' networkExpression
	)* ';'? '}';

networkExpression:
	inputOutputNetworkExpression
	| instanceNetworkExpression
	| routes
	| metrics;

inputOutputNetworkExpression:
	inputs = idList '|' outputs = idList;

instanceNetworkExpression:
	existing = qualifiedId ':' new = idList;

routesList: routes (',' routes)*;
routes:
	qualifiedId '->' (routes | qualifiedId | probabilityIdList);

probabilityIdList:
	'[' expression qualifiedId (',' expression qualifiedId)* ']';

metrics: '*' (metric (',' metric)*)? '*';
metric: namedMetric | functionMetric = qualifiedId;
namedMetric:
	'mrt'
	| 'vrt'
	| 'util'
	| 'throughput'
	| 'num'
	| 'avgNum';

simulateDefinition:
	'simulate' '{' 'run:' network = qualifiedId ';' 'until:' terminationCriteria = expression ';'
		'times:' runs = expression ';'? '}';

statement:
	whileStatement statement?
	| variableDeclarationStatement statement?
	| assignStatement statement?
	| ifStatement statement?
	| returnStatement;

whileStatement:
	'while' condition = expression 'do' body = block;

variableDeclarationStatement: type assignStatement;

assignStatement:
	<assoc = right> identifier '=' expression ';';

ifStatement:
	'if' ifCondition = expression ifBody = block elseIfStatements += elseIfStatement* elseStatement?
		;
elseIfStatement: 'else if' condition = expression body = block;
elseStatement: 'else' body = block;

block: '{' statement? '}';

returnStatement: 'return' expression ';';

expressionList: expression (',' expression)*;

expression: logicalOrExpression;

logicalOrExpression:
	logicalAndExpression ('||' logicalAndExpression)*;

logicalAndExpression:
	equalityExpression ('&&' equalityExpression)*;

equalityExpression: equalExpression | inEqualExpression;

equalExpression:
	relationalExpression ('==' relationalExpression)*;
inEqualExpression:
	relationalExpression ('!=' relationalExpression)*;

relationalExpression:
	lessThanExpression
	| lessThanOrEqualExpression
	| greaterThanExpression
	| greaterThanOrEqualExpression;

lessThanExpression:
	additiveExpression ('<' additiveExpression)*;
lessThanOrEqualExpression:
	additiveExpression ('<=' additiveExpression)*;
greaterThanExpression:
	additiveExpression ('>' additiveExpression)*;
greaterThanOrEqualExpression:
	additiveExpression ('>=' additiveExpression)*;

additiveExpression: addExpression | subtractExpression;

addExpression:
	multiplicativeExpression ('+' multiplicativeExpression)*;
subtractExpression:
	multiplicativeExpression ('-' multiplicativeExpression)*;

multiplicativeExpression:
	multiplyExpression
	| divisionExpression;

multiplyExpression: unaryExpression ('*' unaryExpression)*;
divisionExpression: unaryExpression ('/' unaryExpression)*;

unaryExpression:
	negationExpression
	| negativeExpression
	| parenthesesExpression
	| value;

negationExpression: '!' expression;
negativeExpression: '-' expression;
parenthesesExpression: '(' expression ')';

value:
	functionCall
	| qualifiedId
	| string
	| double
	| int
	| bool
	| arrayInitialization
	| arrayIndexing;

functionCall:
	functionIdentifier = qualifiedId '(' parameters = expressionList? ')';

arrayInitialization: '{' expression* (',' expression)* '}';
arrayIndexing: target = qualifiedId '[' index = expression ']';

type: typeKeyword | arrayType;

typeKeyword:
	boolKeyword
	| intKeyword
	| doubleKeyword
	| stringKeyword;

boolKeyword: 'bool';
intKeyword: 'int';
doubleKeyword: 'double';
stringKeyword: 'string';

arrayType: '[' type ']';

qualifiedIdList: qualifiedId (',' qualifiedId)*;
qualifiedId: identifier ('.' identifier)*;

idList: identifier (',' identifier)*;
identifier: IDENTIFIER;
IDENTIFIER: [a-zA-Z][a-zA-Z0-9_]*;

bool: BOOL;
BOOL: 'true' | 'false';

int: INT;
INT: '-'? [0-9]+;

double: DOUBLE;
DOUBLE: '-'? [0-9]* '.' [0-9]+;

string: STRING;
STRING: '"' ~["\\\r\n]* '"';

WS: (WHITESPACE | TABS | NEWLINES)+ -> skip;
WHITESPACE: ' ';
TABS: '\t';
NEWLINES: '\n' | '\r';

COMMENTS: (ONE_LINE_COMMENT | MULTI_LINE_COMMENT)+ -> skip;
ONE_LINE_COMMENT: '//' ~[\r\n];
MULTI_LINE_COMMENT: '/*' .*? '*/';