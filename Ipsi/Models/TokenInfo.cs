namespace Ipsi.Models
{
    public class TokenInfo
    {
        // - JWT Token : 인증용 토큰
        //     1. Iss : JWT 인증을 해주고자 하는 인증 서비스 ( [https://apis.donga.ac.kr/auth](https://apis.donga.ac.kr/auth) )
        //     2. Aud : JWT 인증받고자 하는 서비스명( 학생정보서비스인 경우 [https://student.donga.ac.kr](https://student.donga.ac.kr) )
        //     3. Sub : 사용자 ID
        //     4. Name : 사용자 성명
        //     5. UserDiv : 학생/교직원/기타 대상자 여부
        //     6. Jti : 토큰 고유번호 
        //     7. Iat: 토큰 발급시간 
        //     8. Exp : 토큰 만료일자 - refresh 값을 이용해 갱신 가능 ( 디폴트 : 발급 후 10분 )
        //     9. Des : 토큰 최종 만료 일자 ( 디폴트 : 3일 )
        // - RefreshToken : 토큰 갱신용 토큰 값
        // - UserName : 사용자 성명

        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserId { get; set; }
        public int UserType { get; set; }
        public string UserName { get; set; }

    }
}