   M S B A M L   `   `   `  ���� K  GBiliLiveRecorder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:BiliLiveRecorderBiliLiveRecorder  "    BiliLiveRecorder.LiveMonitor   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     0localclr-namespace:BiliLiveRecorder     -     � Title$  LiveMonitor��$��325.5��$	��800�� � 
ResizeMode$ CanMinimize=�.��� .+��� $��用户信息：��$
��Left=�$��	10,10,0,0q�$	��Top=��� $��UID：��$
��Left=�$��
117,44,0,0q�$	��Top=��� -    ��Name$	 UID��$
��Left=�$��
171,44,0,0q�$	��Top=�$��78���� $��	昵称：��$
��Left=�$��
117,76,0,0q�$	��Top=��� -   $
 Name��$
��Left=�$��
171,76,0,0q�$	��Top=�$	��200���� $��	性别：��$
��Left=�$��117,109,0,0q�$	��Top=��� -   $	 Sex��$
��Left=�$��171,109,0,0q�$	��Top=�$��78���� -    ��Name$
 Face��$
��Left=�$	��100��$��	10,44,0,0q�$	��Top=�$	��100���� $��	等级：��$
��Left=�$��
254,44,0,0q�$	��Top=��� -   $ Level��$
��Left=�$��
308,44,0,0q�$	��Top=��� $��	生日：��$
��Left=�$��254,109,0,0q�$	��Top=��� -   $ Birthday��$
��Left=�$��308,109,0,0q�$	��Top=��� $��	签名：��$
��Left=�$��
23,155,0,0q�$	��Top=��� $��直播信息：��$
��Left=�$��
387,10,0,0q�$	��Top=��� $��直播间ID：��$
��Left=�$��
430,44,0,0q�$	��Top=��� -   $ RoomID��$
��Left=�$��
509,44,0,0q�$	��Top=��� $��直播间标题：��$
��Left=�$��
430,76,0,0q�$	��Top=��� -	   $ 	RoomTitle��$
��Left=�$��
517,76,0,0q�$	��Top=��� $��直播状态：��$
��Left=�$��430,110,0,0q�$	��Top=��� -
   $ Status��$
��Left=�$��517,108,0,0q�$	��Top=�$
��Bold"�$��14(��� $+��%关闭本窗口即结束监视/录制��$
��Left=�$��405,203,0,0q�$	��354��$��Center=�$��Center=�$��20(�
���   �$��3q����	#FFF08649$��42��$	��Top=���Z� �� -    ��Name$
 Sign��$
��Left=�$��74,163,0,27q� �� TextWrapping$
 Wrap=�$	��297��$ �� HorizontalScrollBarVisibility$ Disabled=�" �� VerticalScrollBarVisibility$ Visible=� �� 
IsReadOnly . �� $��程序状态：��$
��Left=�$��430,142,0,0q�$	��Top=��� -   $ MonitorStatus��$
��Left=�$��517,140,0,0q�$	��Top=�$
��Bold"�$��14(����	#FF5D33EA