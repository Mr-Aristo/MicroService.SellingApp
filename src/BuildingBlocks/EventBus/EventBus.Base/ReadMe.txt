Birden fazla service bus olacagi icin bu proje Base gorevi gorecek
kullanilacak service buslar;
- RabbitMQ 
- AZURE service bus 

Not:Task Hadle(TIntegreationEvent @event);
event anahtar kelimesi, olay tanımlamak için kullanılır. Bu nedenle, bir değişken veya parametre adı olarak event kullanmak bir derleme hatasına neden olur.
Bu sorunu çözmek için, C# dilinde, anahtar kelimeleri değişken veya parametre adı olarak kullanmak gerektiğinde
@ karakteri ön ek olarak eklenir. @ işareti, C# diline bunun bir değişken veya parametre adı olduğunu
ve anahtar kelime olarak değerlendirilmemesi gerektiğini belirtir.