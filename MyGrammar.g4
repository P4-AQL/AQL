grammar MyGrammar;

prog: stat+ ;
stat: expr NEWLINE
  | ID '=' expr NEWLINE
  ;
expr: expr ('*'|'/') expr
  | expr ('+'|'-') expr
  | INT
  | ID
  | '(' expr ')'
  ;
ID: [a-zA-Z]+ ;
INT: [0-9]+ ;
NEWLINE: '\r'? '\n' ;
WS: [ \t\r\n]+ -> skip ;
