set PROTOC=..\Tools\bin\protoc.exe --java_out=..\Server\democommon\src
set PROTOGEN=..\Tools\bin\protogen.exe -output_directory=..\Client\Assets\Scripts\Protos

%PROTOGEN% ClientProtocol.proto
%PROTOGEN% WarChess.proto

%PROTOC% ClientProtocol.proto
%PROTOC% RpcProtocol.proto
%PROTOC% WarChess.proto


%PROTOLUA% ClientProtocol.proto 
pause