$(function(){
	
	starttime();
})
function starttime(){
        time('2021/7/15');                           // 在一起的时间
        ptimer = setTimeout(starttime,1000);            // 添加计时器
}

    function time(futimg){
        var nowtime = new Date().getTime();             // 现在时间转换为时间戳
        var futruetime =  new Date(futimg).getTime();   // 未来时间转换为时间戳
        var msec = nowtime-futruetime;                  // 毫秒 未来时间-现在时间
        var time = (msec/1000);                         // 毫秒/1000
        var day = parseInt(time/86400);                 // 天  24*60*60*1000 
        var hour = parseInt(time/3600)-24*day;          // 小时 60*60 总小时数-过去的小时数=现在的小时数 
        var minute = parseInt(time%3600/60);            // 分 -(day*24) 以60秒为一整份 取余 剩下秒数 秒数/60 就是分钟数
        var second = parseInt(time%60);                 // 以60秒为一整份 取余 剩下秒数
//              console.log(hour+":"+minute+":"+second)
//              alert(hour)
        $("#timecount").html("亲爱的老婆：彭荣<br>咱俩在一起的时间已经：<br>"+day+"天"+hour+"小时"+minute+"分"+second+"秒"+"了<br><span>一时间不知道从哪说起,真爱来了，我们要好好把握。<br>不管面临多大的压力,不管前面的路如何崎岖.<br>不管经历过什么,我仍坚信最浪漫的事就是和你一起慢慢变老.<br>相信中间的计时器,将永远继续下去,直至数据溢出.</span>");

        return true;
    }