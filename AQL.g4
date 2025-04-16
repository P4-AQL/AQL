grammar AQL;

prog: importList (def)*;
importList 
  : 'import' STRING importList
  | // Epsilon
  ;
def 
  : 'const' type ID '=' expr
  | 'function' type ID '(' paramList ')' '{' funcBody '}'
  | network
  ;
network
  : 'queue' ID '=' '{' serviceCount 'service:' intOrDouble ',' 'capacity:' INT metrics '}'
  | 'network' ID '=' '{' 'inputs:' '[' inputs ']' ',' 'outputs:' '[' outputs ']' ',' 'routes:' '{' routes '}' metrics '}'
  ;

inputs
  : inputOption inputList
  ;
inputList
  : ',' inputOption inputList
  | //Epsilon
  ;
inputOption
  : STRING
  | ID
  ;
outputs
  : STRING outputContB
  ;
outputContB
  : ',' STRING outputContB
  | //Epsilon
  ;

routes
  : ID '->' ID routesB
  | ID '->' '[' routeIDList ']' routesB
  ;
routesB
  : ',' ID '->' ID routesB
  | ',' ID '->' '[' routeIDList ']' routesB
  | //Epsilon
  ;
routeIDList
  : aExpr ID routeIDListB
  ;
routeIDListB
  : ',' aExpr ID routeIDListB
  | //Epsilon
  ;

metrics
  : ',' 'metrics:' '[' metricList ']'
  | //Epsilon
  ;
metricList
  : metric metricListA
  ;
metricListA
  : ',' metric metricListA
  | //Epsilon
  ;
metric
  : 'mrt'
  | 'vrt'
  | 'util'
  | 'throughput'
  | 'num'
  | 'avgNum'
  ;

serviceCount
  : 'numberOfService:' funcCall ','
  | 'numberOfService:' INT ','
  | //Epsilon
  ;
paramList 
  : type ID paramListA
  | //Epsilon
  ;
paramListA 
  : ',' type ID paramListA
  | //Epsilon
  ;
stmt
  : stmtA ';' stmt
  | //Epsilon
  ;
stmtA 
  : 'while' bExpr 'do' stmt
  | ID '=' expr
  | type ID '=' expr
  | 'if' bExpr '{' branchBody '}' else1
  ;
else1
  : elseIf else2
  | //Epsion
  ;
else2
  : 'else {' branchBody '}'
  | //Epsilon
  ;
elseIf
  : 'else if' bExpr '{' branchBody '}' elseIf
  | //Epsilon
  ;
branchBody
  : stmt ';' branchBody
  | 'return' expr
  | //Epsilon
  ;
funcBody
  : stmt ';' funcBody
  | 'return' expr
  ;
expr 
  : aExpr
  | bExpr
  | ID
  | funcCall
  | STRING
  | DOUBLE
  | INT
  | BOOL
  ;
actualParamList 
  : ID paramListA
  | //Epsilon
  ;
actualParamListA 
  : ',' ID paramListA
  | //Epsilon
  ;
funcCall
  : ID '(' actualParamList ')'
  ;

aExpr 
  : aTerm aExpr2 
  ;
aExpr2 
  : '+' aTerm aExpr2 // Change to '+' AExpr?
  | '-' aTerm aExpr2
  | //Epsilon
  ;
aTerm: aFactor aTerm2;
aTerm2 
  : '*' aFactor aTerm2
  | '/' aFactor aTerm2
  | //Epsilon
  ;
aFactor 
  : '(' aExpr ')'
  | '-' intOrDouble
  | intOrDouble
  ;
intOrDouble
  : INT
  | DOUBLE
  | ID
  | funcCall
  ;

bExpr
  : b2 b3
  ;
b1 //Probably make a priority
  : '||' bExpr
  | '&&' bExpr
  ;
b2
  : aExpr aToBExpr
  | '!'bExpr
  | BOOL
  | '(' bExpr ')'
  ;
b3
  : b1
  | //Epsilon
  ;
aToBExpr
  : '<=' aExpr
  | '>=' aExpr
  | '==' aExpr
  | '<' aExpr
  | '>' aExpr
  ;
type
  : 'bool'
  | 'int'
  | 'double'
  | 'string'
  | 'network'
  | '[' type ']'
  | '(' type '->' type ')'
  ;
ID
  : [a-zA-Z_][a-zA-Z0-9_]*
  ;
BOOL
  : 'true'
  | 'false'
  ;
INT
  : [0-9]+
  ;
DOUBLE
  : [0-9]* '.' [0-9]+
  ;
STRING
  : '"' ~["\\\r\n]* '"'
  ;

WS
  : (' ' | '\t' | '\n')+ -> skip
  ;