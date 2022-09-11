#include "iostream"
#include "windows.h"
#include "stdio.h"
using namespace std;
#define PIPE_NAME L"\\\\.\\Pipe\\test"
void  main()
{
	char buffer[1024];
	DWORD WriteNum;
 
	if (WaitNamedPipe(PIPE_NAME, NMPWAIT_WAIT_FOREVER) == FALSE)
	{
		cout << "等待命名管道实例失败！" << endl;
		return;
	}
 
	HANDLE hPipe = CreateFile(PIPE_NAME, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hPipe == INVALID_HANDLE_VALUE)
	{
		cout << "创建命名管道失败！" << endl;
		CloseHandle(hPipe);
		return;
	}
	cout << "与服务器连接成功！" << endl;
	while (1)
	{
		printf(buffer);//等待数据输入
		if (WriteFile(hPipe, buffer, strlen(buffer), &WriteNum, NULL) == FALSE)
		{
			cout << "数据写入管道失败！" << endl;
			break;
		}
	}
	
	cout << "关闭管道！" << endl;
	CloseHandle(hPipe);
	system("pause");
}
